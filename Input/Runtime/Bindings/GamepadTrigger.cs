using System;
using MisterGames.Common.Inputs;
using MisterGames.Input.Global;
using UnityEngine;

namespace MisterGames.Input.Bindings {
    
    [Serializable]
    public sealed class GamepadTrigger : IKeyBinding {
        
        public GamepadSide side;
        [Range(0f, 1f)] public float activation;

        public bool IsActive => IsActivated();

        private bool IsActivated() {
            if (DeviceService.Instance.DualSenseAdapter.HasController()) {
                var state = DeviceService.Instance.DualSenseAdapter.GetInputState();
                float value = side switch {
                    GamepadSide.Left => (float) state.LeftTrigger.TriggerValue,
                    GamepadSide.Right => (float) state.RightTrigger.TriggerValue,
                    _ => throw new ArgumentOutOfRangeException()
                };
                
                return value >= activation;
            }

            return side switch {
                GamepadSide.Left => KeyBinding.JoystickTriggerLeft.IsActive(),
                GamepadSide.Right => KeyBinding.JoystickTriggerRight.IsActive(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
    
}
