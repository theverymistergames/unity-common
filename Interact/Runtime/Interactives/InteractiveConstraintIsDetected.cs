using System;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractiveConstraintIsDetected : IInteractionConstraint {

        public bool shouldBeDetected;

        public bool IsAllowedInteraction(IInteractiveUser user, IInteractive interactive) {
            return shouldBeDetected == user.IsDetected(interactive);
        }
    }

}
