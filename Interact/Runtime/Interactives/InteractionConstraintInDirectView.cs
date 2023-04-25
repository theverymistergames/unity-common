using System;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractionConstraintInDirectView : IInteractionConstraint {

        public bool shouldBeInDirectView;

        public bool IsSatisfied(IInteractiveUser user, IInteractive interactive) {
            return shouldBeInDirectView == user.IsInDirectView(interactive, out _);
        }
    }

}
