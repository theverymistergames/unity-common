using System;
using MisterGames.Actors;
using MisterGames.Character.Collisions;
using MisterGames.Character.Input;
using MisterGames.Character.View;
using MisterGames.Collisions.Utils;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Motion {

    public sealed class CharacterMotionPipeline : MonoBehaviour, IActorComponent, IUpdate {

        [Header("Motion")]
        [SerializeField] [Min(0f)] private float _moveForce;
        [SerializeField] [Min(0f)] private float _noGravityVelocityDamping = 10f;
        [SerializeField] private float _speedCorrectionSide = 0.8f;
        [SerializeField] private float _speedCorrectionBack = 0.6f;
        [SerializeField] private float _inputSmoothing = 20f;
        
        [Header("Friction")]
        [SerializeField] [Min(0f)] private float _frictionGrounded = 0.6f;
        [SerializeField] [Min(0f)] private float _frictionAir = 0f;
        [SerializeField] private PhysicMaterialCombine _frictionCombineGrounded = PhysicMaterialCombine.Average;
        [SerializeField] private PhysicMaterialCombine _frictionCombineAir = PhysicMaterialCombine.Multiply;
        
        [Header("Detection")]
        [SerializeField] [Min(0f)] private float _capsuleCastDistance = 0.1f;
        [SerializeField] private float _capsuleCastRadiusOffset = -0.05f;
        [SerializeField] private float _lowerPointOffset = -0.1f;
        [SerializeField] [Min(1)] private int _maxHits = 12;
        [SerializeField] private LayerMask _layer;

        public Vector3 MotionDirection => GetWorldDir(Vector3.forward);
        public Vector2 MotionInput { get; private set; }
        public Vector3 Velocity { get => _rigidbody.velocity; set => _rigidbody.velocity = value; }
        public Vector3 PreviousVelocity { get; private set; }
        public float MoveForce { get => _moveForce; set => _moveForce = value; }
        public float SpeedMultiplier { get; set; }
        public float SpeedCorrectionBack { get => _speedCorrectionBack; set => _speedCorrectionBack = value; }
        public float SpeedCorrectionSide { get => _speedCorrectionSide; set => _speedCorrectionSide = value; }
        public Vector3 Position { get => _rigidbody.position; set => _rigidbody.position = value; }

        private ITimeSource _timeSource;
        private Rigidbody _rigidbody;
        private CharacterHeadAdapter _head;
        private CharacterInputPipeline _input;
        private CharacterGroundDetector _groundDetector;
        private CapsuleCollider _collider;
        private RaycastHit[] _hits;
        private Vector2 _smoothedInput; 

        void IActorComponent.OnAwake(IActor actor) {
            _collider = actor.GetComponent<CapsuleCollider>();
            _input = actor.GetComponent<CharacterInputPipeline>();
            _rigidbody = actor.GetComponent<Rigidbody>();
            _head = actor.GetComponent<CharacterHeadAdapter>();
            _groundDetector = actor.GetComponent<CharacterGroundDetector>();
            _timeSource = PlayerLoopStage.FixedUpdate.Get();

            if (_collider.material == null) _collider.material = new PhysicMaterial();

            _hits = new RaycastHit[_maxHits];
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
            _groundDetector.FetchResults();
            
            UpdateFriction();
            
            _smoothedInput = Vector2.Lerp(_smoothedInput, MotionInput, dt * _inputSmoothing);
            
            float maxSpeed = CalculateSpeedCorrection(_smoothedInput) * SpeedMultiplier;
            var velocity = _rigidbody.velocity;
            var inputDir = new Vector3(_smoothedInput.x, 0f, _smoothedInput.y);
            var forward = GetWorldDir(inputDir);
            
            PreviousVelocity = velocity;
            
            var force = VectorUtils.ClampAcceleration(forward * _moveForce, velocity, maxSpeed, dt);
            LimitForceByObstacles(forward, ref force);
            
            _rigidbody.AddForce(force, ForceMode.Acceleration);

            if (!_rigidbody.useGravity) {
                _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, Vector3.zero, dt * _noGravityVelocityDamping);
            }
        }

        private Vector3 GetWorldDir(Vector3 localDir) {
            // Gravity is disabled: use character head direction
            if (!_rigidbody.useGravity) return _head.Rotation * localDir;
            
            var info = _groundDetector.CollisionInfo;

            return info.hasContact 
                ? Vector3.ProjectOnPlane(_rigidbody.rotation * localDir, info.normal) 
                : _rigidbody.rotation * localDir;
        }

        private float CalculateSpeedCorrection(Vector2 input) {
            // Moving backwards OR backwards + sideways: apply back correction
            if (input.y < 0) return _speedCorrectionBack;

            // Moving forwards OR forwards + sideways: no adjustment
            if (input.y > 0) return 1f;

            // Moving sideways only: apply side correction
            return _speedCorrectionSide;
        }

        private void LimitForceByObstacles(Vector3 dir, ref Vector3 inputForce) {
            if (dir.IsNearlyZero()) return;

            var pos = _rigidbody.position;
            float radius = _collider.radius + _capsuleCastRadiusOffset;
            float height = _collider.height;
            float halfHeight = height * 0.5f - radius;
            
            var center = _collider.center;
            var up = _collider.transform.up;
            
            var p0 = pos + center + halfHeight * up;
            var p1 = pos + center - halfHeight * up;

            int hitCount = Physics.CapsuleCastNonAlloc(
                p0, 
                p1, 
                radius, 
                dir.normalized, 
                _hits, 
                _capsuleCastDistance, 
                _layer, 
                QueryTriggerInteraction.Ignore
            );
            
            if (!_hits.TryGetMinimumDistanceHit(hitCount, out var hit) || 
                VectorUtils.SignedMagnitudeOfProject(hit.point - p1 - _lowerPointOffset * up, up) < 0f
            ) {
                return;
            }
            
            inputForce = Vector3.ProjectOnPlane(inputForce, hit.normal);
        }

        private void UpdateFriction() {
            var mat = _collider.material;
            
            if (_groundDetector.CollisionInfo.hasContact) {
                mat.frictionCombine = _frictionCombineGrounded;
                mat.dynamicFriction = _frictionGrounded;
                mat.staticFriction = _frictionGrounded;
                return;
            }
            
            mat.frictionCombine = _frictionCombineAir;
            mat.dynamicFriction = _frictionAir;
            mat.staticFriction = _frictionAir;
        }
    }

}
