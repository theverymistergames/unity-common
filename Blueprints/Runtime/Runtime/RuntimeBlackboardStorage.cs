using System.Collections.Generic;
using MisterGames.Blackboards.Core;

namespace MisterGames.Blueprints.Runtime {

    public sealed class RuntimeBlackboardStorage : IRuntimeBlackboardStorage {

        private readonly Dictionary<NodeId, Blackboard> _map;

        private RuntimeBlackboardStorage() { }

        public RuntimeBlackboardStorage(int capacity = 0) {
            _map = new Dictionary<NodeId, Blackboard>(capacity);
        }

        public Blackboard GetBlackboard(NodeId id) {
            return _map[id];
        }

        public void SetBlackboard(NodeId id, Blackboard blackboard) {
            _map[id] = blackboard;
        }

        public void Clear() {
            _map.Clear();
        }
    }

}
