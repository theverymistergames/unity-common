using System;

namespace MisterGames.Blueprints.Core2 {

    public sealed class RuntimeNodeStorage : IRuntimeNodeStorage {

        public int Count { get; private set; }

        private NodeId[] _nodes;

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

        public void AllocateNodes(int count) {
            Array.Resize(ref _nodes, Count + count);
        }
    }

}
