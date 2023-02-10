using System;
using UnityEngine;

namespace MisterGames.Input.Activation {

    [Serializable]
    public sealed class KeyActivationStrategyHold : IKeyActivationStrategy {

        [SerializeField] private float _holdTime;
        
        private bool _isProcessing;
        private float _timer;

        public Action OnUse { set => _onUse = value; }
        private Action _onUse = delegate {  };

        public void OnPressed() {
            _isProcessing = true;
            _timer = 0f;
        }

        public void OnReleased() {
            _isProcessing = false;
            _timer = 0f;
        }

        public void Interrupt() {
            _isProcessing = false;
            _timer = 0f;
        }

        public void OnUpdate(float dt) {
            if (!_isProcessing) return;

            if (_timer > _holdTime) {
                _timer = 0f;
                _isProcessing = false;
                _onUse.Invoke();
                return;
            }
            
            _timer += dt;
        }
    }

}
