using System;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractiveConstraintInDirectView : IInteractionConstraint {

        public bool shouldBeInDirectView;

        public bool IsAllowedInteraction(IInteractiveUser user, IInteractive interactive) {
            return shouldBeInDirectView == user.IsInDirectView(interactive);
        }
    }

}
