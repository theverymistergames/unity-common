using MisterGames.Input.Core;
using UnityEngine.InputSystem;

namespace MisterGames.Input.Actions {
    
    public static class InputActionExtensions {

        public static InputAction Get(this InputActionRef inputActionRef) {
            return InputServices.Storage.GetInputAction(inputActionRef.Guid);
        }
        
        public static InputActionMap Get(this InputMapRef inputMapRef) {
            return InputServices.Storage.GetInputMap(inputMapRef.Guid);
        }
    }
    
}