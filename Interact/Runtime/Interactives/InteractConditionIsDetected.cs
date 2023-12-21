using System;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractConditionIsDetected : IInteractCondition {

        public bool shouldBeDetected;

        public bool IsMatched((IInteractiveUser, IInteractive) context) {
            var (user, interactive) = context;
            return shouldBeDetected == user.IsDetected(interactive);
        }
    }

}
