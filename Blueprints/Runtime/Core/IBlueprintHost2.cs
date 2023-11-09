using MisterGames.Blackboards.Core;
using UnityEngine;

namespace MisterGames.Blueprints {

    public interface IBlueprintHost2 {

        MonoBehaviour Runner { get; }

        Blackboard2 GetBlackboard(BlueprintAsset2 blueprint);
    }

}
