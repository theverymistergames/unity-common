using System;

namespace MisterGames.Blueprints.Core2 {

    public sealed class RuntimeNodeStorage : IRuntimeNodeStorage {

        public int Count { get; private set; }

        private NodeId[] _nodes;

        public NodeId GetNode(int index) {
            return _nodes[index];
        }

        public void AddNode(NodeId id) {
            _nodes[Count++] = id;
        }

        public void AllocateSpace(int nodes) {
            Array.Resize(ref _nodes, Count + nodes);
        }
    }

}
