using System;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractConditionIsInteracting : IInteractCondition {

        public bool shouldBeInInteraction;

        public bool IsMatched((IInteractiveUser, IInteractive) context) {
            var (user, interactive) = context;
            return shouldBeInInteraction == user.IsInteractingWith(interactive);
        }
    }

}
