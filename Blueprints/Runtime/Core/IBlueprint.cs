using MisterGames.Blueprints.Runtime;

namespace MisterGames.Blueprints {

    /// <summary>
    /// An interface to pass into Blueprint Node methods,
    /// so nodes can interact with ports.
    /// </summary>
    public interface IBlueprint {

        /// <summary>
        /// Current host object of the blueprint.
        /// </summary>
        IBlueprintHost2 Host { get; }

        /// <summary>
        /// Invoke exit port of node with passed id.
        /// </summary>
        /// <param name="id">Called blueprint node key</param>
        /// <param name="port">Called blueprint node port</param>
        void Call(NodeId id, int port);

        /// <summary>
        /// Read input port of node with passed id. When this operation is performed,
        /// blueprint searches for the first link to this port.
        /// To read values of all the links connected to this port,
        /// use <see cref="GetLinks"/> and read by link index.
        /// Default value can be passed to return when result is not found.
        /// </summary>
        /// <param name="id">Called blueprint node id</param>
        /// <param name="port">Called blueprint node port</param>
        /// <param name="defaultValue">Default value to be returned when result is not found</param>
        /// <typeparam name="T">Type of the read operation result</typeparam>
        /// <returns>Value of type T, or defaultValue if value was not found</returns>
        T Read<T>(NodeId id, int port, T defaultValue = default);

        /// <summary>
        /// Get link reader for node and port.
        /// Link reader allows to read all links for multiple input data ports.
        /// </summary>
        /// <param name="id">Blueprint node id</param>
        /// <param name="port">Blueprint node port index</param>
        /// <returns>Runtime links iterator</returns>
        LinkReader GetLinks(NodeId id, int port);
    }

}
