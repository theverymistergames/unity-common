using MisterGames.Input.Core;
using UnityEngine.InputSystem;

namespace MisterGames.Input.Actions {
    
    public static class InputActionExtensions {

        public static InputAction Get(this InputActionRef inputActionRef) {
            return InputServices.Mapper.GetInputAction(inputActionRef.Guid);
        }
        
        public static InputActionMap Get(this InputMapRef inputMapRef) {
            return InputServices.Mapper.GetInputMap(inputMapRef.Guid);
        }
        
        public static bool TryGet(this InputActionRef inputActionRef, out InputAction inputAction) {
            inputAction = InputServices.Mapper.GetInputAction(inputActionRef.Guid);
            return inputAction != null;
        }
        
        public static bool TryGet(this InputMapRef inputMapRef, out InputActionMap inputActionMap) {
            inputActionMap = InputServices.Mapper.GetInputMap(inputMapRef.Guid);
            return inputActionMap != null;
        }
    }
    
}