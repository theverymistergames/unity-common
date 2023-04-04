using UnityEngine;

namespace MisterGames.Character.Core2 {
    public class CharacterJumpProcessor : MonoBehaviour, ICharacterJumpProcessor {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] private Vector3 _direction;
        [SerializeField] private float _force;

        public Vector3 Direction {
            get => _direction;
            set => _direction = value;
        }

        public float Force {
            get => _force;
            set => _force = value;
        }

        public void SetEnabled(bool isEnabled) {
            if (isEnabled) {
                _characterAccess.Input.Jump -= HandleJumpInput;
                _characterAccess.Input.Jump += HandleJumpInput;
                return;
            }

            _characterAccess.Input.Jump -= HandleJumpInput;
        }

        private void OnEnable() {
            SetEnabled(true);
        }

        private void OnDisable() {
            SetEnabled(false);
        }

        private void HandleJumpInput() {
            _characterAccess.MotionPipeline.GetProcessor<CharacterProcessorMass>()?.ApplyImpulse(Direction * Force);
        }
    }

}
