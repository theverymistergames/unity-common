using System.Runtime.CompilerServices;

namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// Struct that holds data for blueprint node port link.
    /// </summary>
    public readonly struct BlueprintLink {

        /// <summary>
        /// Factory id of the given blueprint node.
        /// </summary>
        public readonly int factoryId;

        /// <summary>
        /// Blueprint node id.
        /// </summary>
        public readonly int nodeId;

        /// <summary>
        /// Port index of the given blueprint node.
        /// </summary>
        public readonly int port;

        /// <summary>
        /// Amount of connections for the given blueprint node port.
        /// </summary>
        public readonly int connections;

        public BlueprintLink(int factoryId, int nodeId, int port, int connections = 0) {
            this.factoryId = factoryId;
            this.nodeId = nodeId;
            this.port = port;
            this.connections = connections;
        }

        /// <summary>
        /// Convert factory id and node id into blueprint node address.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetNodeAddress() {
            return BlueprintNodeAddress.Create(factoryId, nodeId);
        }

        public override string ToString() {
            return $"{nameof(BlueprintLink)}(factoryId {factoryId}, nodeId {nodeId}, port {port}, connections {connections})";
        }
    }

}
