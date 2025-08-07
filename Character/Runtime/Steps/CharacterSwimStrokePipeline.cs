using MisterGames.Actors;
using MisterGames.Character.Capsule;
using MisterGames.Character.Motion;
using MisterGames.Character.Phys;
using MisterGames.Character.View;
using MisterGames.Common;
using MisterGames.Common.Easing;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Character.Steps {

    public sealed class CharacterSwimStrokePipeline : MonoBehaviour, IActorComponent, IUpdate {

        [Header("Stroke Settings")]
        [SerializeField] private float _armPointLevel = 0f;
        [SerializeField] [Min(0f)] private float _armDistance = 0.3f;
        [SerializeField] private float _armForwardOffset = 0.3f;
        [SerializeField] [Min(0f)] private float _speedMin = 0.1f;
        [SerializeField] [Min(0f)] private float _speedMax = 2f;
        [SerializeField] [Min(0f)] private float _minSpeedDetectDuration = 0.15f;
        [SerializeField] [Min(0f)] private float _skipGroundedDuration = 0.15f;
        [SerializeField] [Min(0f)] private float _strokeLengthMin = 0.4f;
        [SerializeField] [Min(0f)] private float _strokeLengthMultiplier = 2f;
        [SerializeField] private AnimationCurve _strokeLengthBySpeed = EasingType.EaseOutExpo.ToAnimationCurve();
        
        public delegate void StrokeCallback(int arm, float distance, Vector3 point);
        public event StrokeCallback OnStroke = delegate {  };

        public float StrokeProgress => Mathf.Clamp01(_strokeProgress);
        
        private Rigidbody _rigidbody;
        private CharacterGroundDetector _groundDetector;
        private CharacterWaterProcessor _waterProcessor;
        private CharacterCapsulePipeline _capsulePipeline;
        private CharacterViewPipeline _view;

        private float _lastTimeNotGrounded;
        private float _lastTimeMoving;
        private float _strokeProgress = -1;
        private int _arm;

        void IActorComponent.OnAwake(IActor actor) {
            _rigidbody = actor.GetComponent<Rigidbody>();
            _groundDetector = actor.GetComponent<CharacterGroundDetector>();
            _waterProcessor  = actor.GetComponent<CharacterWaterProcessor>();
            _capsulePipeline  = actor.GetComponent<CharacterCapsulePipeline>();
            _view = actor.GetComponent<CharacterViewPipeline>();
        }

        private void OnEnable() {
            PlayerLoopStage.Update.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.Update.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            float time = TimeSources.scaledTime;

            var velocity = _rigidbody.linearVelocity;
            float sqrSpeed = velocity.sqrMagnitude;
            
            if (!_groundDetector.HasContact) _lastTimeNotGrounded = time; 
            if (sqrSpeed >= _speedMin * _speedMin) _lastTimeMoving = time;
            
            if (time > _lastTimeNotGrounded + _skipGroundedDuration || 
                time > _lastTimeMoving + _minSpeedDetectDuration || 
                !_waterProcessor.IsUnderwater) 
            {
                _strokeProgress = -1f;
                return;
            }

            var point = _capsulePipeline.GetColliderTopPoint(_armPointLevel - _capsulePipeline.Radius) + GetArmPointOffset(velocity, _arm);
            float strokeLength = _strokeLengthMin +
                               _strokeLengthMultiplier *
                               _strokeLengthBySpeed.Evaluate(_speedMax <= 0f ? 0f : sqrSpeed / (_speedMax * _speedMax));
            
            if (_strokeProgress is >= 0f and < 1f) {
                _strokeProgress = Mathf.Clamp01(_strokeProgress + sqrSpeed * dt / (strokeLength * strokeLength));
                return;
            }
            
#if UNITY_EDITOR
            if (_showDebugInfo) DebugExt.DrawPointer(point, _strokeProgress >= 0f ? Color.cyan : Color.magenta, 0.2f, duration: 2f);
#endif
            
            _strokeProgress = 0f;
            OnStroke.Invoke(_arm, strokeLength, point);
            _arm = _arm == 0 ? 1 : 0;
        }

        private Vector3 GetArmPointOffset(Vector3 velocity, int foot) {
            var up = _view.BodyRotation * Vector3.up;
            var forward = velocity == Vector3.zero
                ? _view.BodyRotation * Vector3.forward
                : velocity;
            
            return Quaternion.LookRotation(forward, up) *
                   (Vector3.right * (0.5f * (foot == 0 ? _armDistance : -_armDistance)) +
                    Vector3.forward * _armForwardOffset);
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;

        private void OnDrawGizmos() {
            if (!_showDebugInfo || _capsulePipeline == null) return;

            var p = _capsulePipeline.GetColliderTopPoint(_armPointLevel - _capsulePipeline.Radius);
            DebugExt.DrawCrossedPoint(p, _capsulePipeline.Root.rotation, Color.yellow, gizmo: true);
        }
#endif
    }

}
