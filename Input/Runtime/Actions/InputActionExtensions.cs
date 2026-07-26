using MisterGames.Input.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MisterGames.Input.Actions {
    
    public static class InputActionExtensions {

        public static InputAction Get(this InputActionRef inputActionRef) {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                object obj = new();
                InputServices.EnableInputInEditModeForSource(obj, true);
                var result = InputServices.Mapper.GetInputAction(inputActionRef.Guid);
                InputServices.EnableInputInEditModeForSource(obj, false);
                return result;
            }
#endif
            
            return InputServices.Mapper.GetInputAction(inputActionRef.Guid);
        }
        
        public static InputActionMap Get(this InputMapRef inputMapRef) {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                object obj = new();
                InputServices.EnableInputInEditModeForSource(obj, true);
                var result = InputServices.Mapper.GetInputMap(inputMapRef.Guid);
                InputServices.EnableInputInEditModeForSource(obj, false);
                return result;
            }
#endif
            
            return InputServices.Mapper.GetInputMap(inputMapRef.Guid);
        }
    }
    
}