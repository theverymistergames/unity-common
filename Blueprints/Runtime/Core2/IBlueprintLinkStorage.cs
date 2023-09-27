namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// A storage that contains all the blueprint ports links after runtime blueprint compilation.
    /// </summary>
    public interface IBlueprintLinkStorage {

        /// <summary>
        /// Return a link by index in the links array.
        /// Index to use can be retrieved by calling <see cref="IBlueprintLinkStorage.GetLinks"/>.
        /// </summary>
        BlueprintLink GetLink(int index);

        /// <summary>
        /// Get first index in the links array and count of the links holding by passed node and port.
        /// </summary>
        void GetLinks(long id, int port, out int index, out int count);

        /// <summary>
        /// Set a link by index in the links array. Link will be created from passed node id and port.
        /// Index to use can be retrieved by calling <see cref="IBlueprintLinkStorage.AddLinks"/>.
        /// </summary>
        void SetLink(int index, int factoryId, int nodeId, int port);

        /// <summary>
        /// Return index in the links array, starting from which there can be set passed count of links.
        /// Links adding must be performed in ascending order of node ports.
        /// </summary>
        int AddLinks(int factoryId, int nodeId, int port, int count);
    }

}
