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
        /// Get type of the blueprint node. See <see cref="BlueprintSource{TNode}"/>.
        /// </summary>
        Type GetNodeType(int id);

        /// <summary>
        /// Get node struct by ref.
        /// </summary>
        /// <param name="id">Node id</param>
        /// <typeparam name="T">Node type</typeparam>
        /// <returns>Node by ref</returns>
        ref T GetNodeByRef<T>(int id) where T : struct, IBlueprintNode;

        /// <summary>
        /// Get node as interface <see cref="IBlueprintNode"/>.
        /// </summary>
        /// <param name="id">Node id</param>
        /// <returns>Node instance</returns>
        IBlueprintNode GetNodeAsInterface(int id);

        /// <summary>
        /// Get node as string to create copies.
        /// </summary>
        /// <param name="id">Blueprint node id</param>
        /// <returns>String representation of the node</returns>
        string GetNodeAsString(int id);

        /// <summary>
        /// Get index of blueprint node in internal storage to create string path for serialized property.
        /// </summary>
        /// <returns>True if node is found</returns>
        bool TryGetNodePath(int id, out int index);

        /// <summary>
        /// Add new node of type.
        /// </summary>
        /// <param name="nodeType">Type of the node to instantiate. Can be null for struct sources.</param>
        /// <returns>Integer pointer of the node in the data array</returns>
        int AddNode(Type nodeType = null);

        /// <summary>
        /// Add node with copied value from source.
        /// </summary>
        /// <param name="source">Blueprint source of cloned node</param>
        /// <param name="cloneId">Id of cloned blueprint node</param>
        /// <returns>Integer pointer of the node in the data array</returns>
        int AddNodeClone(IBlueprintSource source, int cloneId);

        /// <summary>
        /// Add node with copied value from source.
        /// </summary>
        /// <param name="id">Blueprint node id</param>
        /// <param name="source">Blueprint source of cloned node</param>
        /// <param name="cloneId">Id of cloned blueprint node</param>
        void AddNodeClone(int id, IBlueprintSource source, int cloneId);

        /// <summary>
        /// Add node with copied value from source.
        /// </summary>
        /// <param name="source">Blueprint source of cloned node</param>
        /// <param name="id">Id of target blueprint node</param>
        /// <param name="cloneId">Id of cloned blueprint node</param>
        /// <returns>Integer pointer of the node in the data array</returns>
        void SetNodeClone(int id, IBlueprintSource source, int cloneId);

        /// <summary>
        /// Add node with string representation.
        /// </summary>
        /// <param name="value">String representation of node</param>
        /// <param name="nodeType">Type of the node</param>
        /// <returns>Integer pointer of the node in the data array</returns>
        int AddNodeFromString(string value, Type nodeType);

        /// <summary>
        /// Add node with string representation.
        /// </summary>
        /// <param name="id">Blueprint node id</param>
        /// <param name="value">String representation of node</param>
        /// <param name="nodeType">Type of the node</param>
        void AddNodeFromString(int id, string value, Type nodeType);

        /// <summary>
        /// Set node value with string representation.
        /// </summary>
        /// <param name="id">Target blueprint node id</param>
        /// <param name="value">String representation of node</param>
        /// <param name="nodeType">Type of the node</param>
        void SetNodeFromString(int id, string value, Type nodeType);

        /// <summary>
        /// Removes node with id from the data array.
        /// </summary>
        void RemoveNode(int id);

        /// <summary>
        /// Check if node with id is present.
        /// </summary>
        /// <param name="id">Blueprint node id</param>
        /// <returns>True if contains</returns>
        bool ContainsNode(int id);

        /// <summary>
        /// Remove all nodes.
        /// </summary>
        void Clear();

        /// <summary>
        /// Leave only nodes that exist in the source.
        /// </summary>
        /// <returns>True if changed</returns>
        bool MatchNodesWith(IBlueprintSource source);

        /// <summary>
        /// Copy current blueprint source nodes into source.
        /// </summary>
        /// <param name="source"></param>
        void AdditiveCopyInto(IBlueprintSource source);
    }

}
