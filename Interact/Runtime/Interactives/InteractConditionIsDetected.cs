using System;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractConditionIsDetected : IInteractCondition {

        public bool shouldBeDetected;

        public bool IsMatch((IInteractiveUser, IInteractive) context, float startTime) {
            var (user, interactive) = context;
            return shouldBeDetected == user.IsDetected(interactive);
        }
    }

}
