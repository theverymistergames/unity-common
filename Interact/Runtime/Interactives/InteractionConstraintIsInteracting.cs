using System;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractionConstraintIsInteracting : IInteractionConstraint {

        public bool shouldBeInInteraction;

        public bool IsAllowedInteraction(IInteractiveUser user, IInteractive interactive) {
            return shouldBeInInteraction == user.IsInteractingWith(interactive);
        }
    }

}
