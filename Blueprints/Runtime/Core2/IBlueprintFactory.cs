namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// Blueprint factory is an editor and runtime storage for node data structs.
    /// Each factory contains a generic data array to store user defined structs.
    /// A blueprint node is using factory to get data.
    /// </summary>
    public interface IBlueprintFactory {

        /// <summary>
        /// Blueprint node instance created by <see cref="IBlueprintFactory.CreateNode"/>.
        /// </summary>
        IBlueprintNode Node { get; }

        /// <summary>
        /// Current amount of data elements.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// A getter for node data in form of user defined structs.
        /// </summary>
        /// <param name="id">Called blueprint node id</param>
        /// <returns>Reference to the data struct in the storage.</returns>
        ref T GetData<T>(int id) where T : struct;

        /// <summary>
        /// Add space for a new element. Default value is added to the data array.
        /// </summary>
        /// <returns>Integer pointer of the element in the data array</returns>
        int AddBlueprintNodeData();

        /// <summary>
        /// Add element and set its value by copying the element with id from factory.
        /// </summary>
        /// <returns>Integer pointer of the element in the data array</returns>
        int AddBlueprintNodeDataCopy(IBlueprintFactory factory, int id);

        /// <summary>
        /// Removes an element with id from the data array.
        /// Single remove operation marks the corresponding element as empty, and it is ready to store a new element.
        ///
        /// If removed elements amount in the data array is more than a half,
        /// this operation is followed by <see cref="IBlueprintFactory.OptimizeDataLayout"/> method call.
        /// </summary>
        void RemoveBlueprintNodeData(int id);

        /// <summary>
        /// Creates a string path to the serialized property of an element with id.
        /// </summary>
        /// <returns>Relative path of where the element is stored in the <see cref="IBlueprintFactory"/></returns>
        string GetBlueprintNodeDataPath(int id);

        /// <summary>
        /// Clear all the elements.
        /// </summary>
        void Clear();

        /// <summary>
        /// An operation with the data array,
        /// which is used to remove empty spaces and move alive elements closer to the array start.
        /// </summary>
        void OptimizeDataLayout();

        /// <summary>
        /// Create an instance of the needed node class, which should be derived from <see cref="IBlueprintNode"/>.
        /// </summary>
        /// <returns>New instance of a blueprint node</returns>
        IBlueprintNode CreateNode();
    }

}
