using System;
using MisterGames.Common.Conditions;
using MisterGames.Common.Data;
using MisterGames.Input.Actions;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractionConditionInputKey : ICondition {

        public InputActionKeyEvent keyEvent;
        public Optional<bool> shouldBeFired;
        public Optional<bool> shouldBePressed;

        public bool IsMatched =>
            shouldBeFired.IsEmptyOrEquals(keyEvent.WasFired) &&
            shouldBePressed.IsEmptyOrEquals(keyEvent.IsPressed);
    }

}
