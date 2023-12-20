using System;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractConditionMaxDistance : IInteractCondition {

        [Min(0f)] public float maxDistance;

        public bool IsMatch(IInteractiveUser user, IInteractive interactive) {
            return Vector3.SqrMagnitude(user.Transform.position - interactive.Transform.position) <=
                   maxDistance * maxDistance;
        }
    }

}
