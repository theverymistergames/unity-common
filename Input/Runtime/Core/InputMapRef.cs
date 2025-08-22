using System;
using MisterGames.Common.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MisterGames.Input.Core {
    
    [Serializable]
    public struct InputMapRef {
        
        [SerializeField] internal SerializedGuid guid;

        public InputMapRef(Guid guid) {
            this.guid = new SerializedGuid(guid);
        }
        
        public InputActionMap GetInputMap() {
            return InputSystem.actions.FindActionMap(guid.ToGuid());
        }
    }
    
}