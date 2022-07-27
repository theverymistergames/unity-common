using System;

namespace MisterGames.Interact.Core {

    public enum InteractiveMode {
        Tap,
        WhilePressed,
        ClickOnOff
    }

    public static class InteractiveModes {

        private static readonly IInteractiveMode _tap = new InteractiveModeTap();
        private static readonly IInteractiveMode _whilePressed = new InteractiveModeWhilePressed();
        private static readonly IInteractiveMode _clickOnOff = new InteractiveModeTapClickOnOff();

        public static IInteractiveMode Build(this InteractiveMode mode) {
            return mode switch {
                InteractiveMode.Tap => _tap,
                InteractiveMode.WhilePressed => _whilePressed,
                InteractiveMode.ClickOnOff => _clickOnOff,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public interface IInteractiveMode {
        void OnInputPressedWhileIsInteracting(InteractiveUser user, Interactive interactive);
        void OnInputPressedWhileIsNotInteracting(InteractiveUser user, Interactive interactive);
        void OnInputReleasedWhileIsInteracting(InteractiveUser user, Interactive interactive);
    }

    public struct InteractiveModeTap : IInteractiveMode {
        void IInteractiveMode.OnInputPressedWhileIsInteracting(InteractiveUser user, Interactive interactive) { }

        void IInteractiveMode.OnInputPressedWhileIsNotInteracting(InteractiveUser user, Interactive interactive) {
            interactive.StartInteractByUser(user);
            interactive.StopInteractByUser(user);
        }

        void IInteractiveMode.OnInputReleasedWhileIsInteracting(InteractiveUser user, Interactive interactive) { }
    }

    public struct InteractiveModeWhilePressed : IInteractiveMode {
        void IInteractiveMode.OnInputPressedWhileIsInteracting(InteractiveUser user, Interactive interactive) { }

        void IInteractiveMode.OnInputPressedWhileIsNotInteracting(InteractiveUser user, Interactive interactive) {
            interactive.StartInteractByUser(user);
        }

        void IInteractiveMode.OnInputReleasedWhileIsInteracting(InteractiveUser user, Interactive interactive) {
            interactive.StopInteractByUser(user);
        }
    }

    public struct InteractiveModeTapClickOnOff : IInteractiveMode {
        void IInteractiveMode.OnInputPressedWhileIsInteracting(InteractiveUser user, Interactive interactive) {
            interactive.StopInteractByUser(user);
        }

        void IInteractiveMode.OnInputPressedWhileIsNotInteracting(InteractiveUser user, Interactive interactive) {
            interactive.StartInteractByUser(user);
        }

        void IInteractiveMode.OnInputReleasedWhileIsInteracting(InteractiveUser user, Interactive interactive) { }
    }

}
