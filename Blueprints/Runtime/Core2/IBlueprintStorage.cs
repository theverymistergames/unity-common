namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// An interface that is implemented by <see cref="BlueprintNodeFactory"/>.
    /// It is used to deliver user defined blueprint node data struct.
    /// </summary>
    public interface IBlueprintStorage {

        /// <summary>
        /// A getter for data in form of user defined structs.
        /// </summary>
        /// <returns>Reference to the struct in the storage.</returns>
        ref T Get<T>(int id) where T : struct;
    }

}
