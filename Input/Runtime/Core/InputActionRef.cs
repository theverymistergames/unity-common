using System;
using MisterGames.Common.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MisterGames.Input.Core {
    
    [Serializable]
    public struct InputActionRef {
        
        [SerializeField] internal SerializedGuid guid;

        public InputActionRef(Guid guid) {
            this.guid = new SerializedGuid(guid);
        }
        
        public InputAction GetInputAction() {
            return InputSystem.actions.FindAction(guid.ToGuid());
        }
    }
    
}