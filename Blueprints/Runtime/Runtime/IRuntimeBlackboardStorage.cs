using MisterGames.Blackboards.Core;

namespace MisterGames.Blueprints.Runtime {

    public interface IRuntimeBlackboardStorage {

        Blackboard GetBlackboard(NodeId id);

        void SetBlackboard(NodeId id, Blackboard blackboard);

        void Clear();
    }

}
