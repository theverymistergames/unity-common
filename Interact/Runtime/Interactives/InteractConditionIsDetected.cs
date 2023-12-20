using System;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractConditionIsDetected : IInteractCondition {

        public bool shouldBeDetected;

        public bool IsMatch(IInteractiveUser user, IInteractive interactive) {
            return shouldBeDetected == user.IsDetected(interactive);
        }
    }

}
