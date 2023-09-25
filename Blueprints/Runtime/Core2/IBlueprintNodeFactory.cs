namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// Blueprint node factory is an editor and runtime storage for the node data structs.
    /// Each factory contains a generic data array to store custom user's structs.
    /// A blueprint node is using factory to get needed data.
    /// </summary>
    public interface IBlueprintNodeFactory {

        /// <summary>
        /// Current amount of data elements.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Add space for a new element. Default value is added to the data array.
        /// </summary>
        /// <returns>Integer pointer of the element in the data array</returns>
        int AddElement();

        /// <summary>
        /// Add element and set its value by copying an element from storage <see cref="IBlueprintStorage"/> with id.
        /// </summary>
        /// <returns>Integer pointer of the element in the data array</returns>
        int AddElementCopy(IBlueprintStorage storage, int id);

        /// <summary>
        /// Removes an element with id from the data array.
        /// Single remove operation marks the corresponding element as empty, and it is ready to store a new element.
        ///
        /// If removed elements amount in the data array is more than a half,
        /// this operation is followed by <see cref="IBlueprintNodeFactory.OptimizeDataLayout"/> method call.
        /// </summary>
        void RemoveElement(int id);

        /// <summary>
        /// Creates a string path to the serialized property of an element with id.
        /// </summary>
        /// <returns>Relative path of where the element is stored in the <see cref="IBlueprintNodeFactory"/></returns>
        string GetElementPath(int id);

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

        /// <summary>
        /// Create an instance of this node factory class.
        /// </summary>
        /// <returns>New instance of the node factory</returns>
        IBlueprintNodeFactory CreateFactory();
    }

}
