using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace MisterGames.Input.Core {
    
    public sealed class InputMapper : IInputMapper, IDisposable {

        public IReadOnlyList<InputActionMap> InputMaps { get; private set; }

        private readonly Dictionary<Guid, InputAction> _inputActions = new();
        private readonly Dictionary<Guid, InputActionMap> _inputMaps = new();
        
        public void Initialize(InputActionAsset actionAsset) {
            FetchInputs(actionAsset);
        }
        
        public void Dispose() {
            
        }

        private void FetchInputs(InputActionAsset actionAsset) {
            InputMaps = actionAsset.actionMaps;
            
            actionAsset.Enable();
            
            for (int i = 0; i < InputMaps.Count; i++) {
                var inputMap = InputMaps[i];
                _inputMaps[inputMap.id] = inputMap;
                
                var inputActions = inputMap.actions;
                
                for (int j = 0; j < inputActions.Count; j++) {
                    var inputAction = inputActions[j];
                    _inputActions[inputAction.id] = inputAction;
                }
            }
        }

        public InputAction GetInputAction(Guid guid) {
            return _inputActions.GetValueOrDefault(guid);
        }

        public InputActionMap GetInputMap(Guid guid) {
            return _inputMaps.GetValueOrDefault(guid);
        }
    }
    
}