﻿using System;
using MisterGames.Character.Core;
using MisterGames.Input.Actions;
using UnityEngine;

namespace MisterGames.Character.Input {

    public sealed class CharacterInputPipeline : CharacterPipelineBase, ICharacterInputPipeline {

        [SerializeField] private InputActionVector2 _view;
        [SerializeField] private InputActionVector2 _move;
        [SerializeField] private InputActionKey _crouch;
        [SerializeField] private InputActionKey _crouchToggle;
        [SerializeField] private InputActionKey _run;
        [SerializeField] private InputActionKey _jump;

        public event Action<Vector2> OnViewVectorChanged = delegate {  };
        public event Action<Vector2> OnMotionVectorChanged = delegate {  };

        public event Action RunPressed = delegate {  };
        public event Action RunReleased = delegate {  };

        public bool IsRunPressed => enabled && _run.IsPressed;
        public bool WasRunPressed => enabled && _run.WasPressed;
        public bool WasRunReleased => enabled && _run.WasReleased;

        public event Action CrouchPressed = delegate {  };
        public event Action CrouchReleased = delegate {  };
        public event Action CrouchToggled = delegate {  };

        public bool IsCrouchPressed => enabled && _crouch.IsPressed;
        public bool WasCrouchPressed => enabled && _crouch.WasPressed;
        public bool WasCrouchReleased => enabled && _crouch.WasReleased;
        public bool WasCrouchToggled => enabled && _crouchToggle.WasPressed;

        public event Action JumpPressed = delegate {  };

        public override bool IsEnabled { get => enabled; set => enabled = value; }

        private void OnEnable() {
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

            _run.OnPress -= OnRunPressed;
            _run.OnPress += OnRunPressed;

            _run.OnRelease -= OnRunReleased;
            _run.OnRelease += OnRunReleased;

            _jump.OnPress -= OnJumpPressed;
            _jump.OnPress += OnJumpPressed;
        }

        private void OnDisable() {
            _view.OnChanged -= OnViewChanged;
            _move.OnChanged -= OnMoveChanged;

            _crouch.OnPress -= OnCrouchPressed;
            _crouch.OnRelease -= OnCrouchReleased;
            _crouchToggle.OnPress -= OnCrouchToggled;

            _run.OnPress -= OnRunPressed;
            _run.OnRelease -= OnRunReleased;

            _jump.OnPress -= OnJumpPressed;

            OnViewVectorChanged.Invoke(Vector2.zero);
            OnMotionVectorChanged.Invoke(Vector2.zero);

            CrouchReleased.Invoke();
        }

        private void OnViewChanged(Vector2 delta) {
            OnViewVectorChanged.Invoke(delta);
        }

        private void OnMoveChanged(Vector2 motion) {
            OnMotionVectorChanged.Invoke(motion);
        }

        private void OnCrouchPressed() {
            CrouchPressed.Invoke();
        }

        private void OnCrouchReleased() {
            CrouchReleased.Invoke();
        }

        private void OnCrouchToggled() {
            CrouchToggled.Invoke();
        }

        private void OnRunPressed() {
            RunPressed.Invoke();
        }

        private void OnRunReleased() {
            RunReleased.Invoke();
        }

        private void OnJumpPressed() {
            JumpPressed.Invoke();
        }
    }

}
