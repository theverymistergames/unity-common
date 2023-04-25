using System;
using MisterGames.Common.Data;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractionConstraintIsAllowedInteraction : IInteractionConstraint {

        public Optional<bool> shouldBeReadyToStartInteract;
        public Optional<bool> shouldBeAllowedToStartInteract;
        public Optional<bool> shouldBeAllowedToContinueInteract;

        public bool IsSatisfied(IInteractiveUser user, IInteractive interactive) {
            return shouldBeReadyToStartInteract.IsEmptyOrEquals(interactive.IsReadyToStartInteractWith(user)) &&
                   shouldBeAllowedToStartInteract.IsEmptyOrEquals(interactive.IsAllowedToStartInteractWith(user)) &&
                   shouldBeAllowedToContinueInteract.IsEmptyOrEquals(interactive.IsAllowedToContinueInteractWith(user));
        }
    }

}
