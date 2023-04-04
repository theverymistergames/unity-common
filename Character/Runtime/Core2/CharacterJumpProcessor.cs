using UnityEngine;

namespace MisterGames.Character.Core2 {

    public class CharacterJumpProcessor : MonoBehaviour {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] private Vector3 _direction;
        [SerializeField] private float _force;

        private void OnEnable() {
            _characterAccess.Input.Jump += HandleJumpInput;
        }

        private void OnDisable() {
            _characterAccess.Input.Jump -= HandleJumpInput;
        }

        private void HandleJumpInput() {
            var impulse = _direction * _force;
            _characterAccess.MotionPipeline
                .GetProcessor<CharacterProcessorMass>()
                ?.ApplyImpulse(impulse);
        }
    }

}
