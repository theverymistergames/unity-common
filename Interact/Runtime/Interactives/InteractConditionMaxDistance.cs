using System;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractConditionMaxDistance : IInteractCondition {

        [Min(0f)] public float maxDistance;

        public bool IsMatched((IInteractiveUser, IInteractive) context) {
            var (user, interactive) = context;
            return (user.Transform.position - interactive.Transform.position).sqrMagnitude <= maxDistance * maxDistance;
        }
    }

}
