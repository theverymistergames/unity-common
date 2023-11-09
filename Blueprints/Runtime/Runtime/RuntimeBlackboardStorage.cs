using System.Collections.Generic;
using MisterGames.Blackboards.Core;

namespace MisterGames.Blueprints.Runtime {

    public sealed class RuntimeBlackboardStorage : IRuntimeBlackboardStorage {

        private readonly Dictionary<NodeId, Blackboard2> _map;

        private RuntimeBlackboardStorage() { }

        public RuntimeBlackboardStorage(int capacity = 0) {
            _map = new Dictionary<NodeId, Blackboard2>(capacity);
        }

        public Blackboard2 GetBlackboard(NodeId id) {
            return _map[id];
        }

        public void SetBlackboard(NodeId id, Blackboard2 blackboard) {
            _map[id] = blackboard;
        }

        public void Clear() {
            _map.Clear();
        }
    }

}
