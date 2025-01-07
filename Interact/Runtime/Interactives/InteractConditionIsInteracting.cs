using System;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractConditionIsInteracting : IInteractCondition {

        public bool shouldBeInInteraction;

        public bool IsMatch((IInteractiveUser, IInteractive) context, float startTime) {
            var (user, interactive) = context;
            return shouldBeInInteraction == user.IsInteractingWith(interactive);
        }
    }

}
