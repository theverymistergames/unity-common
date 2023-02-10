using System;
using UnityEngine;

namespace MisterGames.Input.Activation {

    [Serializable]
    public sealed class KeyActivationStrategyTap : IKeyActivationStrategy {

        [SerializeField] private float _activationTime;

        public Action OnUse { set => _onUse = value; }
        private Action _onUse = delegate {  };

        private bool _isProcessing;
        private float _timer;

        public void OnPressed() {
            _isProcessing = true;
            _timer = 0f;
        }

        public void OnReleased() { }

        public void Interrupt() {
            _isProcessing = false;
            _timer = 0f;
        }

        public void OnUpdate(float dt) {
            if (!_isProcessing) return;
            
            if (_timer > _activationTime) {
                _timer = 0f;
                _isProcessing = false;
                _onUse.Invoke();
                return;
            }
            
            _timer += dt;
        }
        
    }

}
