using System;
using MisterGames.Common.Data;
using MisterGames.Input.Actions;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractiveConstraintInputKey : IInteractionConstraint {

        public InputActionKeyEvent keyEvent;
        public Optional<bool> shouldBeFired;
        public Optional<bool> shouldBePressed;

        public bool IsAllowedInteraction(IInteractiveUser user, IInteractive interactive) {
            return shouldBeFired.IsEmptyOrEquals(keyEvent.WasFired) &&
                   shouldBePressed.IsEmptyOrEquals(keyEvent.IsPressed);
        }
    }

}
