using System.Diagnostics;
using MisterGames.Character.Collisions;
using MisterGames.Character.Core;
using MisterGames.Character.Motion;
using MisterGames.Collisions.Core;
using MisterGames.Common.Easing;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Maths;
using MisterGames.Dbg.Draw;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Steps {

    public sealed class CharacterStepsPipeline : CharacterPipelineBase, ICharacterStepsPipeline, IUpdate {

        [SerializeField] private PlayerLoopStage _playerLoopStage = PlayerLoopStage.Update;
        [SerializeField] private CharacterAccess _characterAccess;

        [Header("Step Settings")]
        [SerializeField] [Min(0f)] private float _feetDistance = 0.3f;
        [SerializeField] private float _feetForwardOffset = 0.3f;
        [SerializeField] [Min(0f)] private float _speedMax = 10f;
        [SerializeField] [Min(0f)] private float _stepLengthMin = 0.4f;
        [SerializeField] [Min(0f)] private float _stepLengthMultiplier = 3f;
        [SerializeField] private AnimationCurve _stepLengthBySpeed = EasingType.EaseOutExpo.ToAnimationCurve();

        public event StepCallback OnStep = delegate {  };

        private CharacterProcessorMass _mass;
        private ICollisionDetector _groundDetector;
        private ITransformAdapter _body;

        private float _stepProgress;
        private int _foot;

        private void Awake() {
            _mass = _characterAccess.GetPipeline<ICharacterMotionPipeline>().GetProcessor<CharacterProcessorMass>();
            _groundDetector = _characterAccess.GetPipeline<ICharacterCollisionPipeline>().GroundDetector;
            _body = _characterAccess.BodyAdapter;
        }

        private void OnEnable() {
            SetEnabled(true);
        }

        private void OnDisable() {
            SetEnabled(false);
        }

        public override void SetEnabled(bool isEnabled) {
            if (isEnabled) {
                TimeSources.Get(_playerLoopStage).Subscribe(this);
                return;
            }

            TimeSources.Get(_playerLoopStage).Unsubscribe(this);
        }

        public void OnUpdate(float dt) {
            var groundInfo = _groundDetector.CollisionInfo;

            if (!groundInfo.hasContact) {
                _stepProgress = 0f;
                return;
            }

            var velocity = _mass.CurrentVelocity;
            float sqrSpeed = velocity.sqrMagnitude;
            float stepLength = _stepLengthMin +
                               _stepLengthMultiplier *
                               _stepLengthBySpeed.Evaluate(_speedMax < 0f ? 0f : sqrSpeed / (_speedMax * _speedMax));

            if (sqrSpeed.IsNearlyZero() || _stepLengthMultiplier.IsNearlyZero()) {
                _stepProgress = 0f;
                return;
            }

            if (_stepProgress < stepLength * stepLength) {
                _stepProgress += sqrSpeed * dt;
                return;
            }

            _stepProgress = 0f;

            int foot = _foot;
            _foot = _foot == 0 ? 1 : 0;


            var up = _body.Rotation * Vector3.up;
            var dir = Quaternion.LookRotation(Vector3.ProjectOnPlane(velocity, up), up);

            var point =
                groundInfo.point +
                dir *
                (Vector3.right * (0.5f * (_foot == 0 ? _feetDistance : -_feetDistance)) +
                 Vector3.forward * _feetForwardOffset);

            OnStep.Invoke(foot, stepLength, point);

#if UNITY_EDITOR
            DbgDrawStep(point);
#endif
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _debugDrawStep;

        [Conditional("UNITY_EDITOR")]
        private void DbgDrawStep(Vector3 point) {
            if (_debugDrawStep) {
                DbgPointer.Create().Color(Color.yellow).Time(2f).Position(point).Size(0.5f).Draw();
            }
        }
#endif
    }

}
