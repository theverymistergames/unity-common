using System.Collections.Generic;
using MisterGames.Input.Activation;
using UnityEngine;

namespace MisterGames.Input.Core {

    [CreateAssetMenu(fileName = nameof(InputChannel), menuName = "MisterGames/Input/" + nameof(InputChannel))]
    public sealed class InputChannel : InputBase {

        [SerializeField] private InputScheme[] _initialInputSchemes;
        
        private readonly List<InputScheme> _schemes = new List<InputScheme>();
        private readonly List<InputAction> _actions = new List<InputAction>();
        private readonly List<InputAction> _actionsToAdd = new List<InputAction>();
        private readonly List<InputAction> _actionsToRemove = new List<InputAction>();
        private readonly KeyOverlapResolver _overlap = new KeyOverlapResolver();

        public void ActivateInputScheme(InputScheme scheme) {
            if (_schemes.Contains(scheme)) return;
            _schemes.Add(scheme);

            var schemeInputActions = scheme.InputActions;
            for (int i = 0; i < schemeInputActions.Length; i++) {
                var action = schemeInputActions[i];
                AddInputAction(action);
            }
        }
        
        public void DeactivateInputScheme(InputScheme scheme) {
            if (!_schemes.Contains(scheme)) return;
            _schemes.Remove(scheme);
            
            var schemeInputActions = scheme.InputActions;
            for (int i = 0; i < schemeInputActions.Length; i++) {
                var action = schemeInputActions[i];
                RemoveInputAction(action);
            }
        }
        
        public void AddInputAction(InputAction action) {
            if (_actionsToAdd.Contains(action)) return;

            if (_actionsToRemove.Contains(action)) {
                _actionsToRemove.Remove(action);
                return;
            }
            
            _actionsToAdd.Add(action);
        }
        
        public void RemoveInputAction(InputAction action) {
            if (_actionsToRemove.Contains(action)) return;
            
            if (_actionsToAdd.Contains(action)) {
                _actionsToAdd.Remove(action);
                return;
            }
            
            _actionsToRemove.Add(action);
        }

        protected override void OnInit() {
            for (int i = 0; i < _initialInputSchemes.Length; i++) {
                ActivateInputScheme(_initialInputSchemes[i]);    
            }
            CheckInputActionsUpdated();
        }

        protected override void OnTerminate() {
            for (int i = 0; i < _actions.Count; i++) {
                _actions[i].Terminate();
            }
            
            _schemes.Clear();
            _actions.Clear();
            _actionsToAdd.Clear();
            _actionsToRemove.Clear();
            _overlap.Clear();
        }

        protected override void OnUpdate(float dt) {
            for (int i = 0; i < _actions.Count; i++) {
                _actions[i].DoUpdate(dt);
            }
            
            _overlap.ResolveOverlap();
            CheckInputActionsUpdated();
        }

        protected override void OnActivated() {
            for (int i = 0; i < _actions.Count; i++) {
                _actions[i].Activate();
            }
        }

        protected override void OnDeactivated() {
            for (int i = 0; i < _actions.Count; i++) {
                _actions[i].Deactivate();
            }
        }

        private void CheckInputActionsUpdated() {
            bool wasUpdated = false;
            
            for (int i = 0; i < _actionsToAdd.Count; i++) {
                var action = _actionsToAdd[i];
                if (_actions.Contains(action)) continue;
                
                _actions.Add(action);
                UpdateInputActionState(action);
                
                wasUpdated = true;
            }
            _actionsToAdd.Clear();

            for (int i = 0; i < _actionsToRemove.Count; i++) {
                var action = _actionsToRemove[i];
                if (!_actions.Contains(action)) continue;
                
                _actions.Remove(action);
                action.Deactivate();
                
                wasUpdated = true;
            }
            _actionsToRemove.Clear();

            if (wasUpdated) _overlap.RefillOverlapGroups(_actions);
        }

        private void UpdateInputActionState(InputAction action) {
            switch (CurrentStage) {
                case Stage.Initialized:
                    action.Init();
                    break;
                    
                case Stage.Active:
                    action.Activate();
                    break;
                    
                case Stage.Inactive:
                    action.Deactivate();
                    break;
                
                case Stage.Terminated:
                    action.Terminate();
                    break;
            }
        }
    }

}
