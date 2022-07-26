using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Lists;
using MisterGames.Input.Activation;
using MisterGames.Input.Bindings;
using MisterGames.Input.Core;
using UnityEngine;

namespace MisterGames.Input.Actions {

    [CreateAssetMenu(fileName = nameof(InputActionKey), menuName = "MisterGames/Input/Action/" + nameof(InputActionKey))]
    public sealed class InputActionKey : InputAction {

        [SerializeField] private InputBindingKeyBase[] _bindings;
        [SerializeField] private KeyActivationStrategy _strategy;

        public event Action OnUse = delegate {  };
        public event Action OnPress = delegate {  };
        public event Action OnRelease = delegate {  };
        
        public bool IsPressed { get; private set; }
        private bool _hasStrategy;

        protected override void OnInit() {
            foreach (var binding in _bindings) {
                binding.Init();   
            }

            _hasStrategy = _strategy != null;
        }

        protected override void OnTerminate() {
            foreach (var binding in _bindings) {
                binding.Terminate();   
            }
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

        internal bool IsBindingActive() {
            return IsPressed;
        }
        
        internal InputBindingKeyBase[] GetBindings() {
            return _bindings;
        }

        private void HandleUse() {
            OnUse.Invoke();
        }
        
        private void CheckPressState() {
            bool wasPressed = IsPressed;
            IsPressed = _bindings.Some(binding => binding.IsActive());

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