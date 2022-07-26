using UnityEngine;

namespace MisterGames.Input.Activation {

    [CreateAssetMenu(fileName = nameof(KeyActivationStrategyHold), menuName = "MisterGames/Input/Activation/" + nameof(KeyActivationStrategyHold))]
    internal class KeyActivationStrategyHold : KeyActivationStrategy {

        [SerializeField] private float _holdTime;
        
        private bool _isProcessing;
        private float _timer;

        internal override void OnPressed() {
            _isProcessing = true;
            _timer = 0f;
        }

        internal override void OnReleased() {
            _isProcessing = false;
            _timer = 0f;
        }

        internal override void Interrupt() {
            _isProcessing = false;
            _timer = 0f;
        }

        internal override void OnUpdate(float dt) {
            if (!_isProcessing) return;

            if (_timer > _holdTime) {
                _timer = 0f;
                _isProcessing = false;
                FireOnUse();
                return;
            }
            
            _timer += dt;
        }
        
    }

}