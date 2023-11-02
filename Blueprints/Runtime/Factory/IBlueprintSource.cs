using System;

namespace MisterGames.Blueprints.Factory {

    /// <summary>
    /// Blueprint source is an editor and runtime storage for nodes.
    /// Each source contains a generic data array to store user defined node structs.
    /// Blueprint source implements <see cref="IBlueprintNode"/> to be able to pass calls to nodes without
    /// boxing.
    /// </summary>
    public interface IBlueprintSource : IBlueprintNode {

        /// <summary>
        /// Current amount of nodes.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Type of the blueprint node. See <see cref="BlueprintSource{TNode}"/>.
        /// </summary>
        Type NodeType { get; }

        /// <summary>
        /// Get node struct by ref.
        /// </summary>
        /// <param name="id">Node id</param>
        /// <typeparam name="T">Node type</typeparam>
        /// <returns>Node by ref</returns>
        ref T GetNode<T>(int id) where T : struct, IBlueprintNode;

        /// <summary>
        /// Add space for a new node.
        /// </summary>
        /// <returns>Integer pointer of the node in the data array</returns>
        int AddNode();

        /// <summary>
        /// Set node value by copying node from source.
        /// </summary>
        /// <param name="id">Blueprint node id</param>
        /// <param name="source">Blueprint source of copied node</param>
        /// <param name="copyId">Id of copied blueprint node</param>
        void SetNode(int id, IBlueprintSource source, int copyId);

        /// <summary>
        /// Set node value by string representation of node.
        /// </summary>
        /// <param name="id">Blueprint node id</param>
        /// <param name="str">String representation of node</param>
        void SetNode(int id, string str);

        /// <summary>
        /// Get node as string to create copies.
        /// </summary>
        /// <param name="id">Blueprint node id</param>
        /// <returns></returns>
        string GetNodeAsString(int id);

        /// <summary>
        /// Removes node with id from the data array.
        /// </summary>
        void RemoveNode(int id);

        /// <summary>
        /// Get index of blueprint node in internal storage to create string path for serialized property.
        /// </summary>
        /// <returns>True if node is found</returns>
        bool TryGetNodePath(int id, out int index);

        /// <summary>
        /// Remove all nodes.
        /// </summary>
        void Clear();
    }

}
