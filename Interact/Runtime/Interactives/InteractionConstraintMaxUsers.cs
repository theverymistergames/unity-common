using System;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractionConstraintMaxUsers : IInteractionConstraint {

        [Min(0)] public int maxUsers;

        public bool IsSatisfied(IInteractiveUser user, IInteractive interactive) {
            return interactive.Users.Count <= maxUsers;
        }
    }

}
