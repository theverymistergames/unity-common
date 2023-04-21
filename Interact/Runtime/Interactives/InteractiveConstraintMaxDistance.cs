using System;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractiveConstraintMaxDistance : IInteractionConstraint {

        [Min(0f)] public float maxDistance;

        public bool IsAllowedInteraction(IInteractiveUser user, IInteractive interactive) {
            float sqrDistance = Vector3.SqrMagnitude(user.Transform.position - interactive.Transform.position);
            return sqrDistance <= maxDistance * maxDistance;
        }
    }

}
