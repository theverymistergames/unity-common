using MisterGames.Blackboards.Core;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    public interface IBlueprintHost2 {

        MonoBehaviour Runner { get; }

        Blackboard Blackboard { get; }

        Blackboard GetBlackboard(BlueprintAsset2 blueprint);
    }

}
