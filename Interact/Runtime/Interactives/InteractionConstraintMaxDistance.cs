using System;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractionConstraintMaxDistance : IInteractionConstraint {

        [Min(0f)] public float maxDistance;

        public bool IsSatisfied(IInteractiveUser user, IInteractive interactive) {
            float sqrDistance = Vector3.SqrMagnitude(user.Transform.position - interactive.Transform.position);
            return sqrDistance <= maxDistance * maxDistance;
        }
    }

}
