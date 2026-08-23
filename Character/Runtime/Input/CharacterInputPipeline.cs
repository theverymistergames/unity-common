using System;
using MisterGames.Actors;
using MisterGames.Input.Actions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MisterGames.Character.Input {

    public sealed class CharacterInputPipeline : MonoBehaviour, IActorComponent {

        [SerializeField] private InputActionRef _view;
        [SerializeField] private InputActionRef _move;
        [SerializeField] private InputActionRef _crouch;
        [SerializeField] private InputActionRef _crouchToggle;
        [SerializeField] private InputActionRef _run;
        [SerializeField] private InputActionRef _jump;

        public event Action<Vector2> OnMotionVectorChanged = delegate {  };

        public event Action OnCrouchPressed = delegate {  };
        public event Action OnCrouchReleased = delegate {  };
        public event Action OnCrouchToggled = delegate {  };

        public bool IsRunPressed => _enabled && _inputEnabled && _run.Get().IsPressed();

        public event Action JumpPressed = delegate {  };
        public bool IsJumpPressed => _enabled && _inputEnabled && _jump.Get().IsPressed();
        
        public bool IsInputEnabled => _enabled && _inputEnabled;
        public bool IsViewInputEnabled => _enabled && _viewEnabled;

        private InputAction _viewAction;
        private bool _inputEnabled = true;
        private bool _viewEnabled = true;
        private bool _enabled;

        private void OnEnable() {
            _enabled = true;
            if (_inputEnabled) Subscribe();
        }

        private void OnDisable() {
            _enabled = false;
            Unsubscribe();
        }

        public void EnableAllInputs(bool enable) {
            _inputEnabled = enable;
            if (enable) Subscribe();
            else Unsubscribe();
        }
        
        public void EnableViewInputs(bool enable) {
            _viewEnabled = enable;
        }

        public Vector2 GetViewInputVector() {
            return _enabled && _viewEnabled ? (_viewAction ??= _view.Get()).ReadValue<Vector2>() : Vector2.zero;
        }

        private void Subscribe() {
            _move.Get().performed -= HandleMoveChanged;
            _move.Get().performed += HandleMoveChanged;
            
            _move.Get().canceled -= HandleMoveChanged;
            _move.Get().canceled += HandleMoveChanged;

            _crouch.Get().performed -= HandleCrouchPressed;
            _crouch.Get().performed += HandleCrouchPressed;

            _crouch.Get().canceled -= HandleCrouchReleased;
            _crouch.Get().canceled += HandleCrouchReleased;

            _crouchToggle.Get().performed -= HandleCrouchToggled;
            _crouchToggle.Get().performed += HandleCrouchToggled;

            _jump.Get().performed -= HandleJumpPressed;
            _jump.Get().performed += HandleJumpPressed;
        }

        private void Unsubscribe() {
            _move.Get().performed -= HandleMoveChanged;

            _crouch.Get().performed -= HandleCrouchPressed;
            _crouch.Get().canceled -= HandleCrouchReleased;
            _crouchToggle.Get().performed -= HandleCrouchToggled;

            _jump.Get().performed -= HandleJumpPressed;
            
            OnMotionVectorChanged.Invoke(Vector2.zero);
        }

        private void HandleMoveChanged(InputAction.CallbackContext callbackContext) => OnMotionVectorChanged.Invoke(callbackContext.ReadValue<Vector2>());
        private void HandleCrouchPressed(InputAction.CallbackContext callbackContext) => OnCrouchPressed.Invoke();
        private void HandleCrouchReleased(InputAction.CallbackContext callbackContext) => OnCrouchReleased.Invoke();
        private void HandleCrouchToggled(InputAction.CallbackContext callbackContext) => OnCrouchToggled.Invoke();
        private void HandleJumpPressed(InputAction.CallbackContext callbackContext) => JumpPressed.Invoke();
    }

}
