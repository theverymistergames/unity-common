using System;
using MisterGames.Common.Data;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractConditionIsAllowedInteract : IInteractCondition {

        public Optional<bool> shouldBeReadyToStartInteract;
        public Optional<bool> shouldBeAllowedToStartInteract;
        public Optional<bool> shouldBeAllowedToContinueInteract;

        public bool IsMatch((IInteractiveUser, IInteractive) context) {
            var (user, interactive) = context;
            return shouldBeReadyToStartInteract.IsEmptyOrEquals(interactive.IsReadyToStartInteractWith(user)) &&
                   shouldBeAllowedToStartInteract.IsEmptyOrEquals(interactive.IsAllowedToStartInteractWith(user)) &&
                   shouldBeAllowedToContinueInteract.IsEmptyOrEquals(interactive.IsAllowedToContinueInteractWith(user));
        }
    }

}
