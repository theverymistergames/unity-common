using System;
using MisterGames.Actors;
using MisterGames.Input.Actions;
using UnityEngine;

namespace MisterGames.Character.Input {

    public sealed class CharacterInputPipeline : MonoBehaviour, IActorComponent {

        [SerializeField] private InputActionVector2 _view;
        [SerializeField] private InputActionVector2 _move;
        [SerializeField] private InputActionKey _crouch;
        [SerializeField] private InputActionKey _crouchToggle;
        [SerializeField] private InputActionKey _run;
        [SerializeField] private InputActionKey _jump;

        public event Action<Vector2> OnViewVectorChanged = delegate {  };
        public event Action<Vector2> OnMotionVectorChanged = delegate {  };

        public event Action OnRunPressed = delegate {  };
        public event Action OnRunReleased = delegate {  };

        public bool IsRunPressed => enabled && _run.IsPressed;

        public event Action OnCrouchPressed = delegate {  };
        public event Action OnCrouchReleased = delegate {  };
        public event Action OnCrouchToggled = delegate {  };

        public bool IsCrouchPressed => enabled && _crouch.IsPressed;
        public bool WasCrouchToggled => enabled && _crouchToggle.WasPressed;

        public event Action JumpPressed = delegate {  };

        public void EnableViewInput(bool enable) {
            _view.OnChanged -= HandleViewChanged;
            if (enable) _view.OnChanged += HandleViewChanged;
        }
        
        private void OnEnable() {
            Subscribe();
        }

        private void OnDisable() {
            Unsubscribe();
        }

        private void OnDestroy() {
            Unsubscribe();
        }

        private void Subscribe() {
            _view.OnChanged -= HandleViewChanged;
            _view.OnChanged += HandleViewChanged;

            _move.OnChanged -= HandleMoveChanged;
            _move.OnChanged += HandleMoveChanged;

            _crouch.OnPress -= HandleCrouchPressed;
            _crouch.OnPress += HandleCrouchPressed;

            _crouch.OnRelease -= HandleCrouchReleased;
            _crouch.OnRelease += HandleCrouchReleased;

            _crouchToggle.OnPress -= HandleCrouchToggled;
            _crouchToggle.OnPress += HandleCrouchToggled;

            _run.OnPress -= HandleRunPressed;
            _run.OnPress += HandleRunPressed;

            _run.OnRelease -= HandleRunReleased;
            _run.OnRelease += HandleRunReleased;

            _jump.OnPress -= HandleJumpPressed;
            _jump.OnPress += HandleJumpPressed;
        }
        
        private void Unsubscribe() {
            _view.OnChanged -= HandleViewChanged;
            _move.OnChanged -= HandleMoveChanged;

            _crouch.OnPress -= HandleCrouchPressed;
            _crouch.OnRelease -= HandleCrouchReleased;
            _crouchToggle.OnPress -= HandleCrouchToggled;

            _run.OnPress -= HandleRunPressed;
            _run.OnRelease -= HandleRunReleased;

            _jump.OnPress -= HandleJumpPressed;
            
            OnMotionVectorChanged.Invoke(Vector2.zero);
        }

        private void HandleViewChanged(Vector2 delta) => OnViewVectorChanged.Invoke(delta);
        private void HandleMoveChanged(Vector2 motion) => OnMotionVectorChanged.Invoke(motion);

        private void HandleCrouchPressed() => OnCrouchPressed.Invoke();
        private void HandleCrouchReleased() => OnCrouchReleased.Invoke();
        private void HandleCrouchToggled() => OnCrouchToggled.Invoke();

        private void HandleRunPressed() => OnRunPressed.Invoke();
        private void HandleRunReleased() => OnRunReleased.Invoke();

        private void HandleJumpPressed() => JumpPressed.Invoke();
    }

}
