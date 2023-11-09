using MisterGames.Blackboards.Core;
using UnityEngine;

namespace MisterGames.Blueprints {

    public interface IBlueprintHost2 {

        MonoBehaviour Runner { get; }

        Blackboard GetBlackboard(BlueprintAsset2 blueprint);
    }

}
