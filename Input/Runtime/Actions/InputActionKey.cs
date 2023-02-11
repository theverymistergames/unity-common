using System;
using MisterGames.Common.Attributes;
using MisterGames.Input.Activation;
using MisterGames.Input.Bindings;
using MisterGames.Input.Core;
using UnityEngine;

namespace MisterGames.Input.Actions {

    [CreateAssetMenu(fileName = nameof(InputActionKey), menuName = "MisterGames/Input/Action/" + nameof(InputActionKey))]
    public sealed class InputActionKey : InputAction {

        [SerializeReference] [SubclassSelector] private IKeyActivationStrategy _strategy;
        [SerializeReference] [SubclassSelector] private IKeyBinding[] _bindings;

        public event Action OnUse = delegate {  };
        public event Action OnPress = delegate {  };
        public event Action OnRelease = delegate {  };
        
        public bool IsPressed { get; private set; }

        public IKeyBinding[] Bindings {
            get => _bindings;
            set => _bindings = value;
        }

        private bool _hasStrategy;

        protected override void OnInit() {
            _hasStrategy = _strategy != null;
        }

        protected override void OnTerminate() {
            if (_hasStrategy) _strategy.Interrupt();
        }

        protected override void OnActivated() {
            IsPressed = false;
            if (_hasStrategy) _strategy.OnUse = HandleUse;
        }

        protected override void OnDeactivated() {
            IsPressed = false;
            if (_hasStrategy) {
                _strategy.Interrupt();
                _strategy.OnUse = delegate {  };
            }
        }

        protected override void OnUpdate(float dt) {
            CheckPressState();
            if (_hasStrategy) _strategy.OnUpdate(dt);
        }

        internal void Interrupt() {
            if (_hasStrategy) _strategy.Interrupt();
        }

        private void HandleUse() {
            OnUse.Invoke();
        }
        
        private void CheckPressState() {
            bool wasPressed = IsPressed;

            bool hasAtLeastOneActiveBinding = false;
            for (int i = 0; i < _bindings.Length; i++) {
                if (!_bindings[i].IsActive) continue;

                hasAtLeastOneActiveBinding = true;
                break;
            }

            IsPressed = hasAtLeastOneActiveBinding;

            if (!wasPressed && IsPressed) {
                OnPress.Invoke();
                if (_hasStrategy) _strategy.OnPressed();
                return;
            }

            if (wasPressed && !IsPressed) {
                OnRelease.Invoke();
                if (_hasStrategy) _strategy.OnReleased();
            }
        }
        
    }

}
