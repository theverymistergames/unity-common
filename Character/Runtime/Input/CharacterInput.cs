using System;
using MisterGames.Input.Actions;
using UnityEngine;

namespace MisterGames.Character.Input {

    public class CharacterInput : MonoBehaviour {

        [SerializeField] private InputActionVector2 _inputActionView;
        [SerializeField] private InputActionVector2 _inputActionMove;
        [SerializeField] private InputActionKey _inputActionRun;
        [SerializeField] private InputActionKey _inputActionRunToggle;
        [SerializeField] private InputActionKey _inputActionJump;
        [SerializeField] private InputActionKey _inputActionCrouch;
        [SerializeField] private InputActionKey _inputActionCrouchToggle;
        [SerializeField] private InputActionKey _inputActionTorch;

        public event Action<Vector2> View = delegate {  }; 
        public event Action<Vector2> Move = delegate {  }; 
        public event Action StartRun = delegate {  }; 
        public event Action StopRun = delegate {  }; 
        public event Action ToggleRun = delegate {  }; 
        public event Action StartCrouch = delegate {  }; 
        public event Action StopCrouch = delegate {  }; 
        public event Action ToggleCrouch = delegate {  };
        public event Action Jump = delegate {  }; 
        public event Action Torch = delegate {  }; 
        
        private void OnEnable() {
            _inputActionMove.OnChanged += HandleInputMove;
            _inputActionView.OnChanged += HandleInputView;
            
            _inputActionRun.OnPress += HandleInputStartRun;
            _inputActionRun.OnRelease += HandleInputStopRun;
            _inputActionRunToggle.OnPress += HandleInputToggleRun;
            
            _inputActionCrouch.OnPress += HandleInputStartCrouch;
            _inputActionCrouch.OnRelease += HandleInputStopCrouch;
            _inputActionCrouchToggle.OnPress += HandleInputToggleCrouch;
            
            _inputActionJump.OnPress += HandleInputStartJump;
            
            _inputActionTorch.OnPress += HandleInputTorch;
        }

        private void OnDisable() {
            _inputActionMove.OnChanged -= HandleInputMove;
            _inputActionView.OnChanged -= HandleInputView;
            
            _inputActionRun.OnPress -= HandleInputStartRun;
            _inputActionRun.OnRelease -= HandleInputStopRun;
            _inputActionRunToggle.OnPress -= HandleInputToggleRun;
            
            _inputActionCrouch.OnPress -= HandleInputStartCrouch;
            _inputActionCrouch.OnRelease -= HandleInputStopCrouch;
            _inputActionCrouchToggle.OnPress -= HandleInputToggleCrouch;
            
            _inputActionJump.OnPress -= HandleInputStartJump;
            
            _inputActionTorch.OnPress -= HandleInputTorch;
        }

        private void HandleInputView(Vector2 delta) {
            View.Invoke(delta);
        }
        
        private void HandleInputMove(Vector2 direction) {
            Move.Invoke(direction);
        }

        private void HandleInputStartRun() {
            StartRun.Invoke();
        }
        
        private void HandleInputStopRun() {
            StopRun.Invoke();
        }
        
        private void HandleInputToggleRun() {
            ToggleRun.Invoke();
        }
        
        private void HandleInputStartCrouch() {
            StartCrouch.Invoke();
        }
        
        private void HandleInputStopCrouch() {
            StopCrouch.Invoke();
        }
        
        private void HandleInputToggleCrouch() {
            ToggleCrouch.Invoke();
        }
        
        private void HandleInputStartJump() {
            Jump.Invoke();
        }
        
        private void HandleInputTorch() {
            Torch.Invoke();
        }
        
    }

}