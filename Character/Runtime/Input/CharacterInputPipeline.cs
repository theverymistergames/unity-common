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

        public event Action<Vector2> OnViewVectorChanged = delegate {  };
        public event Action<Vector2> OnMotionVectorChanged = delegate {  };

        public event Action OnRunPressed = delegate {  };
        public event Action OnRunReleased = delegate {  };

        public bool IsRunPressed => enabled && _run.Get().IsPressed();

        public event Action OnCrouchPressed = delegate {  };
        public event Action OnCrouchReleased = delegate {  };
        public event Action OnCrouchToggled = delegate {  };

        public bool IsCrouchPressed => enabled && _crouch.Get().IsPressed();
        public bool WasCrouchToggled => enabled && _crouchToggle.Get().WasPressedThisFrame();

        public event Action JumpPressed = delegate {  };
        public bool IsJumpPressed => enabled && _jump.Get().IsPressed();

        public void EnableViewInput(bool enable) {
            _view.Get().performed -= HandleViewChanged;
            if (enable) _view.Get().performed += HandleViewChanged;
        }
        
        private void OnEnable() {
            Subscribe();
        }

        private void OnDisable() {
            Unsubscribe();
        }

        private void Subscribe() {
            _view.Get().performed -= HandleViewChanged;
            _view.Get().performed += HandleViewChanged;

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

            _run.Get().performed -= HandleRunPressed;
            _run.Get().performed += HandleRunPressed;

            _run.Get().canceled -= HandleRunReleased;
            _run.Get().canceled += HandleRunReleased;

            _jump.Get().performed -= HandleJumpPressed;
            _jump.Get().performed += HandleJumpPressed;
        }
        
        private void Unsubscribe() {
            _view.Get().performed -= HandleViewChanged;
            _move.Get().performed -= HandleMoveChanged;

            _crouch.Get().performed -= HandleCrouchPressed;
            _crouch.Get().canceled -= HandleCrouchReleased;
            _crouchToggle.Get().performed -= HandleCrouchToggled;

            _run.Get().performed -= HandleRunPressed;
            _run.Get().canceled -= HandleRunReleased;

            _jump.Get().performed -= HandleJumpPressed;
            
            OnMotionVectorChanged.Invoke(Vector2.zero);
        }

        private void HandleViewChanged(InputAction.CallbackContext callbackContext) => OnViewVectorChanged.Invoke(callbackContext.ReadValue<Vector2>());
        private void HandleMoveChanged(InputAction.CallbackContext callbackContext) => OnMotionVectorChanged.Invoke(callbackContext.ReadValue<Vector2>());

        private void HandleCrouchPressed(InputAction.CallbackContext callbackContext) => OnCrouchPressed.Invoke();
        private void HandleCrouchReleased(InputAction.CallbackContext callbackContext) => OnCrouchReleased.Invoke();
        private void HandleCrouchToggled(InputAction.CallbackContext callbackContext) => OnCrouchToggled.Invoke();

        private void HandleRunPressed(InputAction.CallbackContext callbackContext) => OnRunPressed.Invoke();
        private void HandleRunReleased(InputAction.CallbackContext callbackContext) => OnRunReleased.Invoke();

        private void HandleJumpPressed(InputAction.CallbackContext callbackContext) => JumpPressed.Invoke();
    }

}
