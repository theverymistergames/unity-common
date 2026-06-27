using System;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractConditionBool : IInteractCondition {

        public bool allow;

        public bool IsMatch((IInteractiveUser, IInteractive) context) {
            return allow;
        }
    }

}
