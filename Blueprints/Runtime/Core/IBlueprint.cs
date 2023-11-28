using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Runtime;
using UnityEngine;

namespace MisterGames.Blueprints {

    /// <summary>
    /// An interface to pass into Blueprint Node methods,
    /// so nodes can interact with ports.
    /// </summary>
    public interface IBlueprint {

        /// <summary>
        /// Current host object of the blueprint.
        /// </summary>
        MonoBehaviour Host { get; }

        /// <summary>
        /// Get blackboard by root node id.
        /// </summary>
        Blackboard GetBlackboard(NodeId root);

        /// <summary>
        /// Invoke exit port of node with passed id.
        /// </summary>
        /// <param name="token">Called blueprint node token</param>
        /// <param name="port">Called blueprint node port</param>
        void Call(NodeToken token, int port);

        /// <summary>
        /// Read input port of node with passed id. When this operation is performed,
        /// blueprint searches for the first link to this port.
        /// To read values of all the links connected to this port,
        /// use <see cref="GetLinks"/> and read by link index.
        /// Default value can be passed to return when result is not found.
        /// </summary>
        /// <param name="token">Called blueprint node token</param>
        /// <param name="port">Called blueprint node port</param>
        /// <param name="defaultValue">Default value to be returned when result is not found</param>
        /// <typeparam name="T">Type of the read operation result</typeparam>
        /// <returns>Value of type T, or defaultValue if value was not found</returns>
        T Read<T>(NodeToken token, int port, T defaultValue = default);

        /// <summary>
        /// Get link iterator for node and port.
        /// Link iterator allows to read and call links separately, e.g. for multiple input data ports.
        /// </summary>
        /// <param name="token">Blueprint node token</param>
        /// <param name="port">Blueprint node port index</param>
        /// <returns>Runtime links iterator</returns>
        LinkIterator GetLinks(NodeToken token, int port);
    }

}
