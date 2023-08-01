using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Collisions;
using MisterGames.Character.Core;
using MisterGames.Character.Input;
using MisterGames.Character.Motion;
using MisterGames.Collisions.Core;
using MisterGames.Common.Actions;
using MisterGames.Common.Dependencies;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Jump {

    public class CharacterJumpPipeline : CharacterPipelineBase, ICharacterJumpPipeline {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] private Vector3 _direction = Vector3.up;
        [SerializeField] private float _force = 1f;

        [SerializeField] private AsyncActionAsset _jumpReaction;
        [SerializeField] private AsyncActionAsset _landReaction;

        [RuntimeDependency(typeof(ICharacterAccess))]
        [FetchDependencies(nameof(_jumpReaction))]
        [FetchDependencies(nameof(_landReaction))]
        [SerializeField] private DependencyResolver _dependencies;

        public event Action<Vector3> OnJump = delegate {  };

        public Vector3 LastJumpImpulse => _jumpImpulse;
        public Vector3 Direction { get => _direction; set => _direction = value; }
        public float Force { get => _force; set => _force = value; }
        public float ForceMultiplier { get; set; } = 1f;

        private ICollisionDetector _ceilingDetector;
        private CharacterProcessorMass _mass;
        private Vector3 _jumpImpulse;

        private CancellationTokenSource _destroyCts;

        private void Awake() {
            _destroyCts = new CancellationTokenSource();
            _ceilingDetector = _characterAccess.GetPipeline<ICharacterCollisionPipeline>().CeilingDetector;
            _mass = _characterAccess.GetPipeline<ICharacterMotionPipeline>().GetProcessor<CharacterProcessorMass>();

            _dependencies.SetValue<ICharacterAccess>(_characterAccess);
            _dependencies.Resolve(_jumpReaction);
            _dependencies.Resolve(_landReaction, additive: true);
        }

        private void OnDestroy() {
            _destroyCts.Cancel();
            _destroyCts.Dispose();
        }

        public override void SetEnabled(bool isEnabled) {
            var input = _characterAccess.GetPipeline<ICharacterInputPipeline>();
            var groundDetector = _characterAccess.GetPipeline<ICharacterCollisionPipeline>().GroundDetector;

            if (isEnabled) {
                input.JumpPressed -= HandleJumpPressedInput;
                input.JumpPressed += HandleJumpPressedInput;

                groundDetector.OnContact -= OnLanded;
                groundDetector.OnContact += OnLanded;
                return;
            }

            groundDetector.OnContact -= OnLanded;
            input.JumpPressed -= HandleJumpPressedInput;
        }

        private void OnEnable() {
            SetEnabled(true);
        }

        private void OnDisable() {
            SetEnabled(false);
        }

        private async void HandleJumpPressedInput() {
            if (_ceilingDetector.CollisionInfo.hasContact) return;

            _jumpImpulse = ForceMultiplier * _force * _direction;
            if (_jumpImpulse.IsNearlyZero()) return;

            _mass.ApplyImpulse(_jumpImpulse);
            OnJump.Invoke(_jumpImpulse);

            await TryApply(_jumpReaction);
        }

        private async void OnLanded() {
            await TryApply(_landReaction);
        }

        private UniTask TryApply(IAsyncAction action) {
            return action?.Apply(this, _destroyCts.Token) ?? default;
        }
    }

}
