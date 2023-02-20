using MisterGames.Blackboards.Core;
using UnityEngine;

namespace MisterGames.Blueprints {

    public interface IBlueprintHost {
        MonoBehaviour Runner { get; }
        Blackboard Blackboard { get; }

        Blackboard GetBlackboard(BlueprintAsset blueprint);
    }

}
