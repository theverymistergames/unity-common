using System;
using MisterGames.Character.Collisions;
using MisterGames.Character.Core;
using MisterGames.Character.Input;
using MisterGames.Character.Motion;
using MisterGames.Collisions.Core;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Jump {

    public class CharacterJumpPipeline : CharacterPipelineBase, ICharacterJumpPipeline {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] private Vector3 _direction = Vector3.up;
        [SerializeField] private float _force = 1f;

        public event Action<Vector3> OnJump = delegate {  };

        public Vector3 Direction { get => _direction; set => _direction = value; }
        public float Force { get => _force; set => _force = value; }
        public float ForceMultiplier { get; set; } = 1f;

        private ICollisionDetector _ceilingDetector;
        private CharacterProcessorMass _mass;

        private void Awake() {
            _ceilingDetector = _characterAccess.GetPipeline<ICharacterCollisionPipeline>().CeilingDetector;
            _mass = _characterAccess.GetPipeline<ICharacterMotionPipeline>().GetProcessor<CharacterProcessorMass>();
        }

        public override void SetEnabled(bool isEnabled) {
            var input = _characterAccess.GetPipeline<ICharacterInputPipeline>();

            if (isEnabled) {
                input.JumpPressed -= HandleJumpPressedInput;
                input.JumpPressed += HandleJumpPressedInput;
                return;
            }

            input.JumpPressed -= HandleJumpPressedInput;
        }

        private void OnEnable() {
            SetEnabled(true);
        }

        private void OnDisable() {
            SetEnabled(false);
        }

        private void HandleJumpPressedInput() {
            if (_ceilingDetector.CollisionInfo.hasContact) return;

            var impulse = ForceMultiplier * _force * _direction;
            if (impulse.IsNearlyZero()) return;

            _mass.ApplyImpulse(impulse);
            OnJump.Invoke(impulse);
        }
    }

}
