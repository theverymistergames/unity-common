using MisterGames.Blackboards.Core;

namespace MisterGames.Blueprints.Runtime {

    public interface IRuntimeBlackboardStorage {

        Blackboard2 GetBlackboard(NodeId id);

        void SetBlackboard(NodeId id, Blackboard2 blackboard);

        void Clear();
    }

}
