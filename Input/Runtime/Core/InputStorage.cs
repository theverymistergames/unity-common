using System;
using System.Collections.Generic;
using MisterGames.Input.Bindings;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace MisterGames.Input.Core {
    
    public sealed class InputStorage : IInputStorage, IDisposable {

        public IReadOnlyList<InputActionMap> InputMaps { get; private set; }

        private readonly Dictionary<Guid, InputAction> _inputActions = new();
        private readonly Dictionary<Guid, InputActionMap> _inputMaps = new();

        public void Initialize() {
            FetchInputs();
        }
        
        public void Dispose() {
            _inputActions.Clear();
            _inputMaps.Clear();
        }

        private void FetchInputs() {
            InputMaps = InputSystem.actions.actionMaps;
            
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
            if (!_inputActions.TryGetValue(guid, out var action)) {
                action = InputSystem.actions.FindAction(guid);
                _inputActions[guid] = action;
            }

            return action;
        }

        public InputActionMap GetInputMap(Guid guid) {
            if (!_inputMaps.TryGetValue(guid, out var map)) {
                map = InputSystem.actions.FindActionMap(guid);
                _inputMaps[guid] = map;
            }

            return map;
        }
    }
    
}