using MisterGames.Blackboards.Core;

namespace MisterGames.Blueprints.Runtime {

    public interface IRuntimeBlackboardStorage {

        bool TryGet(NodeId id, out Blackboard blackboard);

        void Add(NodeId id, Blackboard blackboard);

        void Clear();

    }

}
