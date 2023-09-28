using System.Collections.Generic;

namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// A storage that contains all the blueprint nodes and ports links.
    /// </summary>
    public interface IBlueprintStorage {

        /// <summary>
        /// Blueprint node id list.
        /// </summary>
        IReadOnlyList<long> Nodes { get; }

        /// <summary>
        /// Add blueprint node by address.
        /// </summary>
        void AddNode(int factoryId, int nodeId);

        /// <summary>
        /// Return a link by index in the links array.
        /// Index to use can be retrieved by calling <see cref="IBlueprintStorage.GetLinks"/>.
        /// </summary>
        BlueprintLink GetLink(int index);

        /// <summary>
        /// Get first index in the links array and count of the links holding by passed node and port.
        /// </summary>
        void GetLinks(long id, int port, out int index, out int count);

        /// <summary>
        /// Set a link by index in the links array. Link will be created from passed node id and port.
        /// Index to use can be retrieved by calling <see cref="IBlueprintStorage.AddLinks"/>.
        /// </summary>
        void SetLink(int index, int factoryId, int nodeId, int port);

        /// <summary>
        /// Return index in the links array, starting from which there can be set passed count of links.
        /// Links adding must be performed in ascending order of node ports.
        /// </summary>
        int AddLinks(int factoryId, int nodeId, int port, int count);
    }

}
