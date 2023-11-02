using System;

namespace MisterGames.Blueprints.Factory {

    /// <summary>
    /// Blueprint factory is a storage for blueprint sources, see <see cref="IBlueprintSource"/>.
    /// </summary>
    public interface IBlueprintFactory {

        /// <summary>
        /// Get blueprint source by id. Id can be retrieved with <see cref="GetOrCreateSource"/> method.
        /// </summary>
        /// <param name="id">Blueprint source id</param>
        /// <returns>Blueprint source</returns>
        IBlueprintSource GetSource(int id);

        /// <summary>
        /// Get blueprint source by type or create and get if source of such type has not been created yet.
        /// </summary>
        /// <param name="sourceType">Type of the blueprint source</param>
        /// <returns>Blueprint source id</returns>
        int GetOrCreateSource(Type sourceType);

        /// <summary>
        /// Remove blueprint source by id.
        /// </summary>
        /// <param name="id">Blueprint source id</param>
        void RemoveSource(int id);

        /// <summary>
        /// Get indices of blueprint source and node in internal storage to create string path for serialized property.
        /// </summary>
        /// <param name="id">Blueprint node id</param>
        /// <param name="sourceIndex">Index of blueprint source in storage</param>
        /// <param name="nodeIndex">Index of blueprint node in storage</param>
        /// <returns>True if node is found</returns>
        bool TryGetNodePath(NodeId id, out int sourceIndex, out int nodeIndex);

        /// <summary>
        /// Remove all sources.
        /// </summary>
        void Clear();
    }

}
