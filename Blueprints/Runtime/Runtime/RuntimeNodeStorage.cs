namespace MisterGames.Blueprints.Runtime {

    public sealed class RuntimeNodeStorage : IRuntimeNodeStorage {

        public int Count { get; private set; }

        private readonly NodeId[] _nodes;

        private RuntimeNodeStorage() { }

        public RuntimeNodeStorage(int capacity = 0) {
            _nodes = new NodeId[capacity];
        }

        public NodeId GetNode(int index) {
            return _nodes[index];
        }

        public void AddNode(NodeId id) {
            _nodes[Count++] = id;
        }

        public override string ToString() {
            return $"{nameof(RuntimeNodeStorage)}(nodes {string.Join(", ", _nodes)})";
        }
    }

}
