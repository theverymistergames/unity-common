using System.Diagnostics;
using MisterGames.Character.Collisions;
using MisterGames.Character.Core;
using MisterGames.Character.Motion;
using MisterGames.Collisions.Core;
using MisterGames.Common;
using MisterGames.Common.Easing;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Steps {

    public sealed class CharacterStepsPipeline : CharacterPipelineBase, ICharacterStepsPipeline, IUpdate {

        [SerializeField] private PlayerLoopStage _playerLoopStage = PlayerLoopStage.Update;
        [SerializeField] private CharacterAccess _characterAccess;

        [Header("Step Settings")]
        [SerializeField] [Min(0f)] private float _feetDistance = 0.3f;
        [SerializeField] private float _feetForwardOffset = 0.3f;
        [SerializeField] [Min(0f)] private float _speedMin = 0.1f;
        [SerializeField] [Min(0f)] private float _speedMax = 10f;
        [SerializeField] [Min(0f)] private float _stepLengthMin = 0.4f;
        [SerializeField] [Min(0f)] private float _stepLengthMultiplier = 3f;
        [SerializeField] private AnimationCurve _stepLengthBySpeed = EasingType.EaseOutExpo.ToAnimationCurve();

        public override bool IsEnabled { get => enabled; set => enabled = value; }

        public event StepCallback OnStep = delegate {  };
        public event StepCallback OnStartMotionStep = delegate {  };

        private CharacterProcessorMass _mass;
        private ICollisionDetector _groundDetector;
        private ITransformAdapter _body;

        private float _stepProgress = -1;
        private int _foot;

        private void Awake() {
            _mass = _characterAccess.GetPipeline<ICharacterMotionPipeline>().GetProcessor<CharacterProcessorMass>();
            _groundDetector = _characterAccess.GetPipeline<ICharacterCollisionPipeline>().GroundDetector;
            _body = _characterAccess.BodyAdapter;
        }

        private void OnEnable() {
            TimeSources.Get(_playerLoopStage).Subscribe(this);
        }

        private void OnDisable() {
            TimeSources.Get(_playerLoopStage).Unsubscribe(this);
        }

        public void OnUpdate(float dt) {
            var groundInfo = _groundDetector.CollisionInfo;

            if (!groundInfo.hasContact) {
                _stepProgress = -1f;
                return;
            }

            var velocity = _mass.CurrentVelocity;
            float sqrSpeed = velocity.sqrMagnitude;
            float stepLength = _stepLengthMin +
                               _stepLengthMultiplier *
                               _stepLengthBySpeed.Evaluate(_speedMax < 0f ? 0f : sqrSpeed / (_speedMax * _speedMax));

            if (sqrSpeed < _speedMin * _speedMin) {
                _stepProgress = -1f;
                return;
            }

            var point = groundInfo.point + GetFootPointOffset(velocity, _foot);

            if (_stepProgress < 0f) {
                _stepProgress = sqrSpeed * dt;
                OnStartMotionStep.Invoke(_foot, stepLength, point);
                _foot = _foot == 0 ? 1 : 0;

#if UNITY_EDITOR
                DbgDrawStep(point, Color.red);
#endif
                return;
            }

            if (_stepProgress < stepLength * stepLength) {
                _stepProgress += sqrSpeed * dt;
                return;
            }

            _stepProgress = 0f;
            OnStep.Invoke(_foot, stepLength, point);
            _foot = _foot == 0 ? 1 : 0;

#if UNITY_EDITOR
            DbgDrawStep(point, Color.yellow);
#endif
        }

        private Vector3 GetFootPointOffset(Vector3 velocity, int foot) {
            var up = _body.Rotation * Vector3.up;
            var velocityProjection = Vector3.ProjectOnPlane(velocity, up);
            var forward = velocityProjection.sqrMagnitude.IsNearlyZero()
                ? _body.Rotation * Vector3.forward
                : velocity;

            return Quaternion.LookRotation(forward, up) *
                   (Vector3.right * (0.5f * (foot == 0 ? _feetDistance : -_feetDistance)) +
                    Vector3.forward * _feetForwardOffset);
        }

        [Header("Debug")]
        [SerializeField] private bool _debugDrawStep;

#if UNITY_EDITOR
        private void DbgDrawStep(Vector3 point, Color color) {
            if (_debugDrawStep) {
                DebugExt.DrawPointer(point, color, 0.5f, duration: 2f);
            }
        }
#endif
    }

}
