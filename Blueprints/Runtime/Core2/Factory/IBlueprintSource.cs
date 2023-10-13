using System;

namespace MisterGames.Blueprints.Core2 {

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
        /// Add space for a new node and set its value by copying the node with id from source.
        /// </summary>
        /// <returns>Integer pointer of the element in the data array</returns>
        int AddNodeCopy(IBlueprintSource source, int id);

        /// <summary>
        /// Removes node with id from the data array.
        /// </summary>
        void RemoveNode(int id);

        /// <summary>
        /// Creates a string path to the serialized property of node with id.
        /// Method works only in the Unity Editor, otherwise <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <returns>Relative path of where the element is stored in the <see cref="IBlueprintSource"/></returns>
        string GetNodePath(int id);

        /// <summary>
        /// Remove all nodes.
        /// </summary>
        void Clear();
    }

}
