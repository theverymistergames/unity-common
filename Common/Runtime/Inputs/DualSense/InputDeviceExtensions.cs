using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.XInput;

namespace MisterGames.Common.Inputs.DualSense {
    
    public static class InputDeviceExtensions {

        public static void GetInputDeviceType(this InputControl control, out InputDeviceType deviceType, out GamepadType gamepadType) {
            switch (control.device) {
                case Gamepad gamepad:
                    deviceType = InputDeviceType.Gamepad;
                    gamepadType = gamepad.GetGamepadType();
                    break;
                
                default:
                    deviceType = InputDeviceType.KeyboardMouse;
                    gamepadType = GamepadType.Default;
                    break;
            }
        }

        public static GamepadType GetGamepadType(this Gamepad gamepad) {
            return gamepad switch {
                XInputController => GamepadType.XInputController,
                DualSenseGamepadHID => GamepadType.DualSenseGamepad,
                DualShockGamepad => GamepadType.DualShockGamepad,
                SwitchProControllerHID => GamepadType.SwitchProController,
                _ => GamepadType.Default
            };
        }
    }
    
}