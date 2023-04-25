using System;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractionConstraintMaxDirectViewDistance : IInteractionConstraint {

        [Min(0f)] public float maxDistance;

        public bool IsSatisfied(IInteractiveUser user, IInteractive interactive) {
            return user.IsInDirectView(interactive, out float distance) && distance <= maxDistance * maxDistance;
        }
    }

}
