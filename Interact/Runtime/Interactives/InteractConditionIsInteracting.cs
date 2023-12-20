using System;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractConditionIsInteracting : IInteractCondition {

        public bool shouldBeInInteraction;

        public bool IsMatch(IInteractiveUser user, IInteractive interactive) {
            return shouldBeInInteraction == user.IsInteractingWith(interactive);
        }
    }

}
