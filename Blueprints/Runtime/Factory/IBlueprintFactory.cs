using System;

namespace MisterGames.Blueprints.Factory {

    /// <summary>
    /// Blueprint factory is a storage for blueprint sources, see <see cref="IBlueprintSource"/>.
    /// </summary>
    public interface IBlueprintFactory {

        /// <summary>
        /// Get blueprint source by id.
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
        /// Create blueprint source with id.
        /// </summary>
        /// <param name="id">Blueprint source id</param>
        /// <param name="sourceType">Blueprint source type</param>
        IBlueprintSource GetOrCreateSource(int id, Type sourceType);

        /// <summary>
        /// Remove blueprint source by id.
        /// </summary>
        /// <param name="id">Blueprint source id</param>
        void RemoveSource(int id);

        /// <summary>
        /// Leave only nodes that exist in the source.
        /// </summary>
        /// <param name="factory">Source factory</param>
        /// <returns>True if changed</returns>
        bool MatchNodesWith(IBlueprintFactory factory);

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

        /// <summary>
        /// Copy current blueprint factory values into factory.
        /// </summary>
        void AdditiveCopyInto(IBlueprintFactory factory);
    }

}
