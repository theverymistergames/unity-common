using System;
using System.Threading;
using MisterGames.Character.Collisions;
using MisterGames.Character.Core;
using MisterGames.Character.Input;
using MisterGames.Character.Motion;
using MisterGames.Collisions.Core;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Jump {

    public sealed class CharacterJumpPipeline : CharacterPipelineBase, ICharacterJumpPipeline {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] private Vector3 _direction = Vector3.up;
        [SerializeField] private float _force = 1f;

        public event Action<Vector3> OnJump = delegate {  };

        public Vector3 LastJumpImpulse => _jumpImpulse;
        public Vector3 Direction { get => _direction; set => _direction = value; }

        public float Force { get => _force; set => _force = value; }
        public float ForceMultiplier { get; set; } = 1f;

        public override bool IsEnabled { get => enabled; set => enabled = value; }

        private ICharacterInputPipeline _input;
        private ICollisionDetector _ceilingDetector;
        private CharacterProcessorMass _mass;
        private Vector3 _jumpImpulse;

        private CancellationTokenSource _destroyCts;

        private void Awake() {
            _destroyCts = new CancellationTokenSource();
            _ceilingDetector = _characterAccess.GetPipeline<ICharacterCollisionPipeline>().CeilingDetector;
            _input = _characterAccess.GetPipeline<ICharacterInputPipeline>();
            _mass = _characterAccess.GetPipeline<ICharacterMotionPipeline>().GetProcessor<CharacterProcessorMass>();
        }

        private void OnDestroy() {
            _destroyCts.Cancel();
            _destroyCts.Dispose();
        }

        private void OnEnable() {
            _input.JumpPressed -= HandleJumpPressedInput;
            _input.JumpPressed += HandleJumpPressedInput;
        }

        private void OnDisable() {
            _input.JumpPressed -= HandleJumpPressedInput;
        }

        private void HandleJumpPressedInput() {
            if (_ceilingDetector.CollisionInfo.hasContact) return;

            _jumpImpulse = ForceMultiplier * _force * _direction;
            if (_jumpImpulse.IsNearlyZero()) return;

            _mass.ApplyImpulse(_jumpImpulse);
            OnJump.Invoke(_jumpImpulse);
        }
    }

}
