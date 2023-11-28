using System.Collections.Generic;
using MisterGames.Blackboards.Core;

namespace MisterGames.Blueprints.Runtime {

    public sealed class RuntimeBlackboardStorage : IRuntimeBlackboardStorage {

        private readonly Dictionary<NodeId, Blackboard> _map;

        private RuntimeBlackboardStorage() { }

        public RuntimeBlackboardStorage(int capacity = 0) {
            _map = new Dictionary<NodeId, Blackboard>(capacity);
        }

        public bool TryGet(NodeId id, out Blackboard blackboard) {
            return _map.TryGetValue(id, out blackboard);
        }

        public void Add(NodeId id, Blackboard blackboard) {
            _map.Add(id, blackboard);
        }

        public void Clear() {
            _map.Clear();
        }
    }

}
