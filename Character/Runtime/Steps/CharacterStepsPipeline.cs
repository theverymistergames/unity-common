using MisterGames.Actors;
using MisterGames.Character.Collisions;
using MisterGames.Character.Motion;
using MisterGames.Character.View;
using MisterGames.Collisions.Core;
using MisterGames.Common;
using MisterGames.Common.Easing;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Character.Steps {

    public sealed class CharacterStepsPipeline : MonoBehaviour, IActorComponent, IUpdate {

        [Header("Step Settings")]
        [SerializeField] [Min(0f)] private float _feetDistance = 0.3f;
        [SerializeField] private float _feetForwardOffset = 0.3f;
        [SerializeField] [Min(0f)] private float _speedMin = 0.1f;
        [SerializeField] [Min(0f)] private float _speedMax = 10f;
        [SerializeField] [Min(0f)] private float _minSpeedDetectDuration = 0.15f;
        [SerializeField] [Min(0f)] private float _skipNotGroundedDuration = 0.15f;
        [SerializeField] [Min(0f)] private float _stepLengthMin = 0.4f;
        [SerializeField] [Min(0f)] private float _stepLengthMultiplier = 3f;
        [SerializeField] private AnimationCurve _stepLengthBySpeed = EasingType.EaseOutExpo.ToAnimationCurve();

        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;
        
        public delegate void StepCallback(int foot, float distance, Vector3 point);
        
        public event StepCallback OnStep = delegate {  };

        public float StepProgress => Mathf.Clamp01(_stepProgress);
        
        private Rigidbody _rigidbody;
        private CharacterGroundDetector _groundDetector;
        private CharacterViewPipeline _view;

        private float _lastTimeGrounded;
        private float _lastTimeMoving;
        private float _stepProgress = -1;
        private int _foot;
        
        public void OnAwake(IActor actor) {
            _rigidbody = actor.GetComponent<Rigidbody>();
            _groundDetector = actor.GetComponent<CharacterGroundDetector>();
            _view = actor.GetComponent<CharacterViewPipeline>();
        }

        private void OnEnable() {
            PlayerLoopStage.Update.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.Update.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            float time = Time.time;
            var velocityFlat = Vector3.ProjectOnPlane(_rigidbody.linearVelocity, _view.BodyRotation * Vector3.up);
            float sqrSpeedFlat = velocityFlat.sqrMagnitude;
            
            if (_groundDetector.HasContact) _lastTimeGrounded = time; 
            if (sqrSpeedFlat >= _speedMin * _speedMin) _lastTimeMoving = time;
            
            if (time - _lastTimeGrounded > _skipNotGroundedDuration || time - _lastTimeMoving > _minSpeedDetectDuration) {
                _stepProgress = -1f;
                return;
            }

            var point = _groundDetector.CollisionInfo.point + GetFootPointOffset(velocityFlat, _foot);
            float stepLength = _stepLengthMin +
                               _stepLengthMultiplier *
                               _stepLengthBySpeed.Evaluate(_speedMax <= 0f ? 0f : sqrSpeedFlat / (_speedMax * _speedMax));
            
            if (_stepProgress is >= 0f and < 1f) {
                _stepProgress = Mathf.Clamp01(_stepProgress + sqrSpeedFlat * dt / (stepLength * stepLength));
                return;
            }
            
#if UNITY_EDITOR
            if (_showDebugInfo) DebugExt.DrawPointer(point, _stepProgress >= 0f ? Color.yellow : Color.red, 0.2f, duration: 2f);
#endif
            
            _stepProgress = 0f;
            OnStep.Invoke(_foot, stepLength, point);
            _foot = _foot == 0 ? 1 : 0;
        }

        private Vector3 GetFootPointOffset(Vector3 velocity, int foot) {
            var up = _view.BodyRotation * Vector3.up;
            var forward = velocity == Vector3.zero
                ? _view.BodyRotation * Vector3.forward
                : velocity;
            
            return Quaternion.LookRotation(forward, up) *
                   (Vector3.right * (0.5f * (foot == 0 ? _feetDistance : -_feetDistance)) +
                    Vector3.forward * _feetForwardOffset);
        }
    }

}
