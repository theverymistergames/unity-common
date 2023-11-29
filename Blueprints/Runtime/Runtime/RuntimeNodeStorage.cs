using System;

namespace MisterGames.Blueprints.Runtime {

    public sealed class RuntimeNodeStorage : IRuntimeNodeStorage {

        public int Count { get; private set; }

        private NodeToken[] _nodeTokens;

        private RuntimeNodeStorage() { }

        public RuntimeNodeStorage(int capacity = 0) {
            _nodeTokens = new NodeToken[capacity];
        }

        public NodeToken GetToken(int index) {
            return _nodeTokens[index];
        }

        public NodeId GetNode(int index) {
            return _nodeTokens[index].node;
        }

        public void AddNode(NodeId id, NodeId root) {
            _nodeTokens[Count++] = new NodeToken(id, root);
        }

        public void AllocateNodes(int count) {
            Array.Resize(ref _nodeTokens, _nodeTokens.Length + count);
        }

        public void Clear() {
            Count = 0;
            _nodeTokens = Array.Empty<NodeToken>();
        }

        public override string ToString() {
            return $"{nameof(RuntimeNodeStorage)}(nodes {string.Join(", ", _nodeTokens)})";
        }
    }

}
