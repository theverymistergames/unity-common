using System;

namespace MisterGames.Blueprints.Core2 {

    public sealed class RuntimeNodeStorage : IRuntimeNodeStorage {

        public int Count { get; private set; }

        private readonly NodeId[] _nodes;

        private RuntimeNodeStorage() { }

        public RuntimeNodeStorage(int count = 0) {
            _nodes = count == 0 ? Array.Empty<NodeId>() : new NodeId[count];
        }

        public NodeId GetNode(int index) {
            return _nodes[index];
        }

        public void AddNode(NodeId id) {
            _nodes[Count++] = id;
        }
    }

}
