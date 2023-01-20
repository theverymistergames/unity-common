using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints {

    public interface IBlueprintHost {

        MonoBehaviour Runner { get; }
        RuntimeBlackboard Blackboard { get; }

    }

}
