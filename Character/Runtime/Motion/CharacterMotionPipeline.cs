using System;
using MisterGames.Actors;
using MisterGames.Character.Collisions;
using MisterGames.Character.Input;
using MisterGames.Character.View;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Motion {

    public sealed class CharacterMotionPipeline : MonoBehaviour, IActorComponent, IUpdate {

        [SerializeField] [Min(0f)] private float _moveForce;
        [SerializeField] [Min(0f)] private float _noGravityVelocityDamping = 10f;
        [SerializeField] private float _speedCorrectionSide = 0.8f;
        [SerializeField] private float _speedCorrectionBack = 0.6f;
        [SerializeField] private float _inputSmoothing = 20f;
        
        public Vector2 MotionInput { get; private set; }
        public Vector3 Velocity { get => _rigidbody.velocity; set => _rigidbody.velocity = value; }
        public Vector3 PreviousVelocity { get; private set; }
        public float MoveForce { get => _moveForce; set => _moveForce = value; }
        public float SpeedMultiplier { get; set; }
        public float SpeedCorrectionBack { get => _speedCorrectionBack; set => _speedCorrectionBack = value; }
        public float SpeedCorrectionSide { get => _speedCorrectionSide; set => _speedCorrectionSide = value; }

        private ITimeSource _timeSource;
        private Rigidbody _rigidbody;
        private CharacterHeadAdapter _head;
        private CharacterInputPipeline _input;
        private CharacterGroundDetector _groundDetector;
        private Func<Vector2,Vector3> _forwardDirConverter;

        private Vector2 _smoothedInput; 

        void IActorComponent.OnAwake(IActor actor) {
            _input = actor.GetComponent<CharacterInputPipeline>();
            _rigidbody = actor.GetComponent<Rigidbody>();
            _head = actor.GetComponent<CharacterHeadAdapter>();
            _groundDetector = actor.GetComponent<CharacterGroundDetector>();
            _timeSource = PlayerLoopStage.FixedUpdate.Get();
        }
        
        private void OnEnable() {
            _input.OnMotionVectorChanged += HandleMotionInput;
            _timeSource.Subscribe(this);
        }

        private void OnDisable() {
            _input.OnMotionVectorChanged -= HandleMotionInput;
            _timeSource.Unsubscribe(this);
        }

        private void HandleMotionInput(Vector2 input) {
            MotionInput = input;
        }

        void IUpdate.OnUpdate(float dt) {
            _smoothedInput = Vector2.Lerp(_smoothedInput, MotionInput, dt * _inputSmoothing);
            
            float maxSpeed = CalculateSpeedCorrection(_smoothedInput) * SpeedMultiplier;
            var forward = GetForwardDir(_smoothedInput);
            var velocity = _rigidbody.velocity;
            
            PreviousVelocity = velocity;
            
            var force = VectorUtils.ClampAcceleration(forward * _moveForce, velocity, maxSpeed, dt);
            _rigidbody.AddForce(force, ForceMode.Acceleration);

            if (!_rigidbody.useGravity) {
                _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, Vector3.zero, dt * _noGravityVelocityDamping);
            }
        }
        
        public void SetOverrideForwardDir(Func<Vector2, Vector3> converter) {
            _forwardDirConverter = converter;
        }

        private Vector3 GetForwardDir(Vector2 input) {
            if (_forwardDirConverter != null) return _forwardDirConverter.Invoke(input);

            var dir = new Vector3(input.x, 0f, input.y);

            _groundDetector.FetchResults();
            var info = _groundDetector.CollisionInfo;

            // Gravity is enabled and has ground contact: use character body direction and consider ground normal
            if (_rigidbody.useGravity && info.hasContact) {
                return Vector3.ProjectOnPlane(_rigidbody.rotation * dir, info.normal);
            }

            // Move direction is same as view direction while gravity is not enabled or has no ground contact
            return _head.Rotation * dir;
        }

        private float CalculateSpeedCorrection(Vector2 input) {
            // Moving backwards OR backwards + sideways: apply back correction
            if (input.y < 0) return _speedCorrectionBack;

            // Moving forwards OR forwards + sideways: no adjustment
            if (input.y > 0) return 1f;

            // Moving sideways only: apply side correction
            return _speedCorrectionSide;
        }
    }

}
