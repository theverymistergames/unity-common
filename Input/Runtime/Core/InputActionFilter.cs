using System;
using System.Collections.Generic;
using MisterGames.Input.Actions;
using UnityEngine;

namespace MisterGames.Input.Core {

    [Serializable]
    public sealed class InputActionFilter {

        [SerializeField] private InputChannel _inputChannel;
        [SerializeField] private InputAction[] _addActions;
        [SerializeField] private InputAction[] _removeActions;

        private bool _isApplied;
        
        public void Apply() {
            if (_isApplied) return;
            
            Deactivate(_removeActions);
            Activate(_addActions);
            _isApplied = true;
        }
        
        public void Release() {
            if (!_isApplied) return;
            
            Deactivate(_addActions);
            Activate(_removeActions);
            _isApplied = false;
        }

        private void Activate(InputAction[] actions) {
            for (int i = 0; i < actions.Length; i++) {
                _inputChannel.AddInputAction(actions[i]);
            }
        }
        
        private void Deactivate(InputAction[] actions) {
            for (int i = 0; i < actions.Length; i++) {
                _inputChannel.RemoveInputAction(actions[i]);
            }
        }
        
    }

}
