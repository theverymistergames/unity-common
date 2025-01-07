using System;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractConditionIsInteractingWithAny : IInteractCondition {

        public bool shouldBeInInteraction;

        public bool IsMatch((IInteractiveUser, IInteractive) context, float startTime) {
            var (user, _) = context;
            return shouldBeInInteraction == user.Interactives.Count > 0;
        }
    }

}
