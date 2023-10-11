using System;

namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// Runtime port address struct, used to search for port links.
    /// </summary>
    public readonly struct RuntimePortAddress : IEquatable<RuntimePortAddress> {

        /// <summary>
        /// Blueprint node id.
        /// </summary>
        public readonly long nodeId;

        /// <summary>
        /// Blueprint port index.
        /// </summary>
        public readonly int port;

        public RuntimePortAddress(long nodeId, int port) {
            this.nodeId = nodeId;
            this.port = port;
        }

        public bool Equals(RuntimePortAddress other) {
            return nodeId == other.nodeId && port == other.port;
        }

        public override bool Equals(object obj) {
            return obj is RuntimePortAddress other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(nodeId, port);
        }
    }

}
