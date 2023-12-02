using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Factory;

namespace MisterGames.Blueprints.Runtime {

    public interface IBlueprintHost {

        int GetSubgraphIndex(NodeId id, int parent = -1);

        Blackboard GetRootBlackboard();

        IBlueprintFactory GetRootFactory();

        Blackboard GetSubgraphBlackboard(NodeId id, int parent = -1);

        IBlueprintFactory GetSubgraphFactory(NodeId id, int parent = -1);
    }

}
