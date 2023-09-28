namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// An interface to pass into Blueprint Node class methods,
    /// for nodes to access their data structs and interact with node ports.
    /// </summary>
    public interface IBlueprint {

        /// <summary>
        /// Current host object of the blueprint.
        /// </summary>
        IBlueprintHost2 Host { get; }

        /// <summary>
        /// A getter for node data in form of user defined structs.
        /// </summary>
        /// <param name="id">Called blueprint node id</param>
        /// <returns>Reference to the data struct in the storage.</returns>
        ref T GetData<T>(long id) where T : struct;

        /// <summary>
        /// Get first index and count of links for passed node id and port.
        /// Results can be used to read all links of the input port with Read by link index method.
        /// </summary>
        /// <param name="id">Blueprint node id</param>
        /// <param name="port">Blueprint node port index</param>
        /// <param name="index">First index of links</param>
        /// <param name="count">Count of links</param>
        void GetLinks(long id, int port, out int index, out int count);

        /// <summary>
        /// Invoke exit port of node with passed id.
        /// </summary>
        /// <param name="id">Called blueprint node id</param>
        /// <param name="port">Called blueprint node port</param>
        void Call(long id, int port);

        /// <summary>
        /// Read input port of node with passed id. When this operation is performed,
        /// blueprint searches for the first link to this port.
        /// To read values of all the links connected to this port,
        /// use <see cref="IBlueprint.GetLinks"/> and read by link index.
        /// Default value can be passed to return when result is not found.
        /// </summary>
        /// <param name="id">Called blueprint node id</param>
        /// <param name="port">Called blueprint node port</param>
        /// <param name="defaultValue">Default value to be returned when result is not found</param>
        /// <typeparam name="T">Type of the read operation result</typeparam>
        /// <returns>Value of type T, or defaultValue if value was not found</returns>
        T Read<T>(long id, int port, T defaultValue = default);

        /// <summary>
        /// Read input port by link index. This operation is useful when you need
        /// to read several input connection to the port. First you need to retrieve
        /// indices of links by calling <see cref="IBlueprint.GetLinks"/>.
        /// Default value can be passed to return when result is not found.
        /// </summary>
        /// <param name="linkIndex">Input port link index</param>
        /// <param name="defaultValue">Default value to be returned when result is not found</param>
        /// <typeparam name="T">Type of the read operation result</typeparam>
        /// <returns>Value of type T, or defaultValue if value was not found</returns>
        T Read<T>(int linkIndex, T defaultValue = default);
    }

}
