using UnityEngine;

namespace MisterGames.Input.Activation {

    [CreateAssetMenu(fileName = nameof(KeyActivationStrategyTap), menuName = "MisterGames/Input/Activation/" + nameof(KeyActivationStrategyTap))]
    internal class KeyActivationStrategyTap : KeyActivationStrategy {

        [SerializeField] private float _activationTime;
        
        private bool _isProcessing;
        private float _timer;

        internal override void OnPressed() {
            _isProcessing = true;
            _timer = 0f;
        }

        internal override void OnReleased() { }

        internal override void Interrupt() {
            _isProcessing = false;
            _timer = 0f;
        }

        internal override void OnUpdate(float dt) {
            if (!_isProcessing) return;
            
            if (_timer > _activationTime) {
                _timer = 0f;
                _isProcessing = false;
                FireOnUse();
                return;
            }
            
            _timer += dt;
        }
        
    }

}