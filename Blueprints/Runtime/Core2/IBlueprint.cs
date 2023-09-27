namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// An interface to read user defined blueprint node data structs, interact with node ports.
    /// </summary>
    public interface IBlueprint {

        /// <summary>
        /// A getter for node data in form of user defined structs.
        /// </summary>
        /// <param name="id">Called blueprint node id</param>
        /// <returns>Reference to the data struct in the storage.</returns>
        ref T GetData<T>(long id) where T : struct;

        /// <summary>
        /// Invoke exit port of node with passed id.
        /// </summary>
        /// <param name="id">Called blueprint node id</param>
        /// <param name="port">Called blueprint node port</param>
        void Call(long id, int port);

        /// <summary>
        /// Read input port of node with passed id.
        /// Default value can be passed to return when result is not found.
        /// </summary>
        /// <param name="id">Called blueprint node id</param>
        /// <param name="port">Called blueprint node port</param>
        /// <param name="defaultValue">Default value to be returned when result is not found</param>
        /// <typeparam name="T">Type of the read operation result</typeparam>
        /// <returns>Value of type R, or default if value was not found</returns>
        T Read<T>(long id, int port, T defaultValue = default);
    }

}
