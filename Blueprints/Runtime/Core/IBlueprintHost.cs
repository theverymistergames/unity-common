using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints {

    public interface IBlueprintHost {
        MonoBehaviour Runner { get; }
        Blackboard Blackboard { get; }

        void ResolveBlackboardSceneReferences(BlueprintAsset blueprint, Blackboard blackboard);
    }

}
