using System;
using MisterGames.Common.Attributes;
using MisterGames.Input.Activation;
using MisterGames.Input.Bindings;
using MisterGames.Input.Core;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Input.Actions {

    [CreateAssetMenu(fileName = nameof(InputActionKey), menuName = "MisterGames/Input/Action/" + nameof(InputActionKey))]
    public sealed class InputActionKey : InputActionBase {

        [SerializeReference] [SubclassSelector] private IKeyActivationStrategy _strategy;
        [SerializeReference] [SubclassSelector] private IKeyBinding[] _bindings;

        public event Action OnUse = delegate {  };
        public event Action OnPress = delegate {  };
        public event Action OnRelease = delegate {  };
        
        public bool IsPressed { get; private set; }

        public bool WasPressed => _lastPressFrame == TimeSources.frameCount;
        public bool WasReleased => _lastReleaseFrame == TimeSources.frameCount;
        public bool WasUsed => _lastUseFrame == TimeSources.frameCount;

        public IKeyBinding[] Bindings {
            get => _bindings;
            set => _bindings = value;
        }

        private int _lastUseFrame;
        private int _lastPressFrame;
        private int _lastReleaseFrame;

        protected override void OnInit() { }

        protected override void OnTerminate() {
            _strategy?.Interrupt();
        }

        protected override void OnActivated() {
            IsPressed = false;

            if (_strategy == null) return;

            _strategy.OnUse = HandleUse;
        }

        protected override void OnDeactivated() {
            IsPressed = false;

            if (_strategy == null) return;

            _strategy.Interrupt();
            _strategy.OnUse = delegate {  };
        }

        protected override void OnUpdate(float dt) {
            CheckPressState();
            _strategy?.OnUpdate(dt);
        }

        internal void Interrupt() {
            _strategy?.Interrupt();
        }

        private void HandleUse() {
            _lastUseFrame = TimeSources.frameCount;
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
                _lastPressFrame = TimeSources.frameCount;
                OnPress.Invoke();
                _strategy?.OnPressed();
                return;
            }

            if (wasPressed && !IsPressed) {
                _lastReleaseFrame = TimeSources.frameCount;
                OnRelease.Invoke();
                _strategy?.OnReleased();
            }
        }
        
    }

}
