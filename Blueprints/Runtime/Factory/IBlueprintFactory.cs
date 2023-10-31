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
        /// Creates a string path to the serialized property of blueprint node with id.
        /// Method works only in the Unity Editor, otherwise <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="id">Blueprint node id</param>
        /// <returns>Relative path of where the element is stored in the blueprint factory.</returns>
        string GetNodePath(NodeId id);

        /// <summary>
        /// Remove all sources.
        /// </summary>
        void Clear();
    }

}
