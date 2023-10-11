namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// Blueprint factory is an editor and runtime storage for node data structs.
    /// Each factory contains a generic data array to store user defined structs.
    /// A blueprint node is using factory to get data.
    /// </summary>
    public interface IBlueprintFactory : IBlueprintNode {

        /// <summary>
        /// Current amount of data elements.
        /// </summary>
        int Count { get; }

        ref T GetNode<T>(int id) where T : struct, IBlueprintNode;

        /// <summary>
        /// Add space for a new element. Default value is added to the data array.
        /// </summary>
        /// <returns>Integer pointer of the element in the data array</returns>
        int AddNode();

        /// <summary>
        /// Add element and set its value by copying the element with id from factory.
        /// </summary>
        /// <returns>Integer pointer of the element in the data array</returns>
        int AddNodeCopy(IBlueprintFactory factory, int id);

        /// <summary>
        /// Removes an element with id from the data array.
        /// Single remove operation marks the corresponding element as empty, and it is ready to store a new element.
        /// </summary>
        void RemoveNode(int id);

        /// <summary>
        /// Creates a string path to the serialized property of an element with id.
        /// </summary>
        /// <returns>Relative path of where the element is stored in the <see cref="IBlueprintFactory"/></returns>
        string GetNodePath(int id);

        /// <summary>
        /// Clear all the elements.
        /// </summary>
        void Clear();
    }

}
