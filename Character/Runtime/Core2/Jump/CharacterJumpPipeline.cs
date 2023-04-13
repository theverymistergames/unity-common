using System;
using MisterGames.Character.Core2.Access;
using MisterGames.Character.Core2.Motion;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Core2.Jump {

    public class CharacterJumpPipeline : MonoBehaviour, ICharacterJumpPipeline {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] private Vector3 _direction = Vector3.up;
        [SerializeField] private float _force = 1f;

        public event Action<Vector3> OnJump = delegate {  };

        public Vector3 Direction { get => _direction; set => _direction = value; }
        public float Force { get => _force; set => _force = value; }
        public float ForceMultiplier { get; set; } = 1f;

        public void SetEnabled(bool isEnabled) {
            if (isEnabled) {
                _characterAccess.Input.JumpPressed -= HandleJumpPressedInput;
                _characterAccess.Input.JumpPressed += HandleJumpPressedInput;
                return;
            }

            _characterAccess.Input.JumpPressed -= HandleJumpPressedInput;
        }

        private void OnEnable() {
            SetEnabled(true);
        }

        private void OnDisable() {
            SetEnabled(false);
        }

        private void HandleJumpPressedInput() {
            if (_characterAccess.CeilingDetector.CollisionInfo.hasContact) return;

            var impulse = ForceMultiplier * _force * _direction;
            if (impulse.IsNearlyZero()) return;

            _characterAccess.MotionPipeline.GetProcessor<CharacterProcessorMass>()?.ApplyImpulse(impulse);
            OnJump.Invoke(impulse);
        }
    }

}
