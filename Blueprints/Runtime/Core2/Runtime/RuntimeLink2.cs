namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// Struct that holds data for blueprint node port link. It is used only in runtime.
    /// </summary>
    public readonly struct RuntimeLink2 {

        /// <summary>
        /// Blueprint node id.
        /// </summary>
        public readonly long nodeId;

        /// <summary>
        /// Port index of the given blueprint node.
        /// </summary>
        public readonly int port;

        /// <summary>
        /// Amount of connections for the given blueprint node port.
        /// </summary>
        public readonly int connections;

        public RuntimeLink2(long nodeId, int port, int connections = 0) {
            this.nodeId = nodeId;
            this.port = port;
            this.connections = connections;
        }

        public override string ToString() {
            return $"{nameof(RuntimeLink2)}(nodeId {nodeId}, port {port}, connections {connections})";
        }
    }

}
