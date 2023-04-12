using System;
using MisterGames.Input.Actions;
using UnityEngine;

namespace MisterGames.Character.Core2.Input {

    public sealed class CharacterInput : MonoBehaviour, ICharacterInput {

        [SerializeField] private InputActionVector2 _view;
        [SerializeField] private InputActionVector2 _move;
        [SerializeField] private InputActionKey _crouch;
        [SerializeField] private InputActionKey _crouchToggle;
        [SerializeField] private InputActionKey _runToggle;
        [SerializeField] private InputActionKey _jump;

        public event Action<Vector2> OnViewVectorChanged = delegate {  };
        public event Action<Vector2> OnMotionVectorChanged = delegate {  };

        public event Action RunToggled = delegate {  };
        public bool WasRunToggled { get; private set; }

        public event Action CrouchPressed = delegate {  };
        public event Action CrouchReleased = delegate {  };
        public event Action CrouchToggled = delegate {  };

        public bool IsCrouchInputActive => _crouch.IsPressed;
        public bool WasCrouchPressed { get; private set; }
        public bool WasCrouchReleased { get; private set; }
        public bool WasCrouchToggled { get; private set; }

        public event Action JumpPressed = delegate {  };
        public bool IsJumpPressed => _jump.IsPressed;

        public void SetEnabled(bool isEnabled) {
            if (isEnabled) {
                _view.OnChanged -= OnViewChanged;
                _view.OnChanged += OnViewChanged;

                _move.OnChanged -= OnMoveChanged;
                _move.OnChanged += OnMoveChanged;

                _crouch.OnPress -= OnCrouchPressed;
                _crouch.OnPress += OnCrouchPressed;

                _crouch.OnRelease -= OnCrouchReleased;
                _crouch.OnRelease += OnCrouchReleased;

                _crouchToggle.OnPress -= OnCrouchToggled;
                _crouchToggle.OnPress += OnCrouchToggled;

                _runToggle.OnPress -= OnRunToggled;
                _runToggle.OnPress += OnRunToggled;

                _jump.OnPress -= OnJumpPressed;
                _jump.OnPress += OnJumpPressed;
                return;
            }

            _view.OnChanged -= OnViewChanged;
            _move.OnChanged -= OnMoveChanged;

            _crouch.OnPress -= OnCrouchPressed;
            _crouch.OnRelease -= OnCrouchReleased;
            _crouchToggle.OnPress -= OnCrouchToggled;

            _runToggle.OnPress -= OnRunToggled;

            _jump.OnPress -= OnJumpPressed;

            OnViewVectorChanged.Invoke(Vector2.zero);
            OnMotionVectorChanged.Invoke(Vector2.zero);

            CrouchReleased.Invoke();
        }

        private void OnEnable() {
            SetEnabled(true);
        }

        private void OnDisable() {
            SetEnabled(false);
        }

        private void OnViewChanged(Vector2 delta) {
            OnViewVectorChanged.Invoke(delta);
        }

        private void OnMoveChanged(Vector2 motion) {
            OnMotionVectorChanged.Invoke(motion);
        }

        private void OnCrouchPressed() {
            WasCrouchPressed = true;
            CrouchPressed.Invoke();
            WasCrouchPressed = false;
        }

        private void OnCrouchReleased() {
            WasCrouchReleased = true;
            CrouchReleased.Invoke();
            WasCrouchReleased = false;
        }

        private void OnCrouchToggled() {
            WasCrouchToggled = true;
            CrouchToggled.Invoke();
            WasCrouchToggled = false;
        }

        private void OnRunToggled() {
            WasRunToggled = true;
            RunToggled.Invoke();
            WasRunToggled = false;
        }

        private void OnJumpPressed() {
            JumpPressed.Invoke();
        }
    }

}
