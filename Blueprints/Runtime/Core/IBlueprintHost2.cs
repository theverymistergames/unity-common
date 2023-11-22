using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Meta;
using UnityEngine;

namespace MisterGames.Blueprints {

    public interface IBlueprintHost2 {

        MonoBehaviour Runner { get; }

        int GetSubgraphIndex(NodeId id, int parent = -1);

        Blackboard GetBlackboard(NodeId id = default, int parent = -1);

        BlueprintMeta2 GetBlueprintMeta(NodeId id = default, int parent = -1);
    }

}
