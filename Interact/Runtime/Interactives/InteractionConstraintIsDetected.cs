using System;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractionConstraintIsDetected : IInteractionConstraint {

        public bool shouldBeDetected;

        public bool IsSatisfied(IInteractiveUser user, IInteractive interactive) {
            return shouldBeDetected == user.IsDetected(interactive);
        }
    }

}
