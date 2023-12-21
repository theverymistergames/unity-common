using System;
using MisterGames.Common.Data;
using MisterGames.Input.Actions;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractConditionInputKey : IInteractCondition {

        public InputActionKeyEvent keyEvent;
        public Optional<bool> shouldBeFired;
        public Optional<bool> shouldBePressed;

        public bool IsMatched((IInteractiveUser, IInteractive) context) {
            return shouldBeFired.IsEmptyOrEquals(keyEvent.WasFired) &&
                   shouldBePressed.IsEmptyOrEquals(keyEvent.IsPressed);
        }
    }

}
