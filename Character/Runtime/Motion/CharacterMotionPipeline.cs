﻿using MisterGames.Actors;
using MisterGames.Character.Collisions;
using MisterGames.Character.Input;
using MisterGames.Character.View;
using MisterGames.Collisions.Utils;
using MisterGames.Common;
using MisterGames.Common.Attributes;
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
        [SerializeField] [MinMaxSlider(0f, 1f)] private Vector2 _forceCorrection = new Vector2(0f, 0.5f);
        [SerializeField] [MinMaxSlider(0f, 180f)] private Vector2 _forceCorrectionAngle = new Vector2(15f, 90f);
        [SerializeField] [MinMaxSlider(0f, 90f)] private Vector2 _slopeAngle = new Vector2(25f, 45f);
        [SerializeField] private float _inputSmoothing = 20f;
        
        [Header("Friction")]
        [SerializeField] [Min(0f)] private float _frictionGrounded = 0.6f;
        [SerializeField] [Min(0f)] private float _frictionAir;
        [SerializeField] [Min(0f)] private float _frictionSlope = 1f;
        [SerializeField] [Min(0f)] private float _frictionSlopeOverMaxAngle = 0.6f;
        [SerializeField] private PhysicMaterialCombine _frictionCombineGrounded = PhysicMaterialCombine.Average;
        [SerializeField] private PhysicMaterialCombine _frictionCombineAir = PhysicMaterialCombine.Multiply;
        [SerializeField] private PhysicMaterialCombine _frictionCombineSlope = PhysicMaterialCombine.Maximum;
        [SerializeField] private PhysicMaterialCombine _frictionCombineSlopeOverMaxAngle = PhysicMaterialCombine.Average;

        [Header("Detection")]
        [SerializeField] [Min(0f)] private float _capsuleCastDistance = 0.1f;
        [SerializeField] private float _capsuleCastRadiusOffset = -0.05f;
        [SerializeField] private float _lowerPointOffset = -0.1f;
        [SerializeField] [Min(1)] private int _maxHits = 12;
        [SerializeField] private LayerMask _layer;

        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;
        
        public Vector3 MotionDirWorld { get; private set; }
        public Vector3 InputDirWorld { get; private set; }
        public Vector2 Input { get; private set; }
        public Vector3 Velocity { get => _rigidbody.velocity; set => _rigidbody.velocity = value; }
        public Vector3 PreviousVelocity { get; private set; }
        public float MoveForce { get => _moveForce; set => _moveForce = value; }
        public float Speed { get; set; }
        public float SpeedCorrectionBack { get => _speedCorrectionBack; set => _speedCorrectionBack = value; }
        public float SpeedCorrectionSide { get => _speedCorrectionSide; set => _speedCorrectionSide = value; }
        public float SlopeAngle { get; private set; }
        public Vector3 Position { get => _rigidbody.position; set => _rigidbody.position = value; }

        private Transform _transform;
        private Rigidbody _rigidbody;
        private CharacterViewPipeline _view;
        private CharacterInputPipeline _input;
        private CharacterGroundDetector _groundDetector;
        private CapsuleCollider _collider;
        private RaycastHit[] _hits;
        private Vector2 _smoothedInput;

        void IActorComponent.OnAwake(IActor actor) {
            _transform = actor.Transform;
                
            _collider = actor.GetComponent<CapsuleCollider>();
            _input = actor.GetComponent<CharacterInputPipeline>();
            _rigidbody = actor.GetComponent<Rigidbody>();
            _view = actor.GetComponent<CharacterViewPipeline>();
            _groundDetector = actor.GetComponent<CharacterGroundDetector>();
            
            if (_collider.material == null) _collider.material = new PhysicMaterial();

            _hits = new RaycastHit[_maxHits];
        }
        
        private void OnEnable() {
            _input.OnMotionVectorChanged += HandleMotionInput;
            PlayerLoopStage.FixedUpdate.Subscribe(this);
        }

        private void OnDisable() {
            _input.OnMotionVectorChanged -= HandleMotionInput;
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
        }

        public void AddForce(Vector3 force, ForceMode mode = ForceMode.Force) {
            _rigidbody.AddForce(force, mode);
        }
        
        private void HandleMotionInput(Vector2 input) {
            Input = input;
        }

        void IUpdate.OnUpdate(float dt) {
            var up = _transform.up;
            var orient = _view.Rotation;

            if (_rigidbody.useGravity) {
                orient = Quaternion.LookRotation(Vector3.ProjectOnPlane(orient * Vector3.forward, up), up);
            }
            
            _smoothedInput = Vector2.Lerp(_smoothedInput, Input, dt * _inputSmoothing);
            InputDirWorld = Input.IsNearlyZero() ? Vector3.zero : orient * InputToLocal(Input).normalized;
            
            _groundDetector.Forward = InputDirWorld;
            _groundDetector.FetchResults();

            MotionDirWorld = GetGroundDir(InputDirWorld, up);
            SlopeAngle = GetSlopeAngle(MotionDirWorld);
            
            if (_rigidbody.isKinematic) return;
            
            float maxSpeed = CalculateSpeedCorrection(Input) * Speed;
            var velocity = _rigidbody.velocity;
            PreviousVelocity = velocity;

            var inputDirNormalized = orient * (_smoothedInput.IsNearlyZero() ? Vector3.forward : InputToLocal(_smoothedInput).normalized);
            var inputDirSmoothed = orient * InputToLocal(_smoothedInput);
            
            var velocityProj = Vector3.Project(velocity, inputDirNormalized);
            var force = VectorUtils.ClampAcceleration(inputDirSmoothed * _moveForce, velocityProj, maxSpeed, dt);

            if (!Input.IsNearlyZero()) {
                LimitForceByObstacles(inputDirNormalized, ref force);
                LimitForceBySlopeAngle(SlopeAngle, ref force);
                ApplyDirCorrection(inputDirNormalized * maxSpeed, velocity, ref force, dt);
            }
            
            _rigidbody.AddForce(force, ForceMode.Acceleration);

#if UNITY_EDITOR
            if (_showDebugInfo) DebugExt.DrawSphere(_rigidbody.position, 0.05f, Color.cyan);
            if (_showDebugInfo) DebugExt.DrawRay(_rigidbody.position, inputDirNormalized, Color.cyan);
#endif
            
            if (!_rigidbody.useGravity) {
                _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, Vector3.zero, dt * _noGravityVelocityDamping);
            }
            
            UpdateFriction(SlopeAngle);
        }

        private static Vector3 InputToLocal(Vector2 input) {
            return new Vector3(input.x, 0f, input.y);
        }

        private Vector3 GetGroundDir(Vector3 worldDir, Vector3 up) {
            return _groundDetector.HasContact 
                ? Quaternion.FromToRotation(up, _groundDetector.CollisionInfo.normal) * worldDir
                : worldDir;
        }
        
        private float CalculateSpeedCorrection(Vector2 input) {
            // Moving backwards OR backwards + sideways: apply back correction
            if (input.y < 0) return _speedCorrectionBack;

            // Moving forwards OR forwards + sideways: no adjustment
            if (input.y > 0) return 1f;

            // Moving sideways only: apply side correction
            return _speedCorrectionSide;
        }

        private void LimitForceBySlopeAngle(float slopeAngle, ref Vector3 inputForce) {
            if (!_groundDetector.HasContact || slopeAngle <= _slopeAngle.y) return;
            
            var info = _groundDetector.CollisionInfo;
            var up = _rigidbody.transform.up;
            var slopeUp = Vector3.Cross(Vector3.Cross(info.normal, up), info.normal).normalized;
            
            inputForce = Vector3.ProjectOnPlane(inputForce, slopeUp);
        }

        private void LimitForceByObstacles(Vector3 direction, ref Vector3 inputForce) {
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
                direction, 
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

        private void ApplyDirCorrection(Vector3 targetVelocity, Vector3 velocity, ref Vector3 force, float dt) {
            if (targetVelocity.IsNearlyZero() || !_groundDetector.HasContact) return;

            var nextVelocity = velocity + force * dt;
            var perfectForce = dt > 0f ? (targetVelocity - velocity) / dt : force;
                
            float angle = Vector3.Angle(targetVelocity, nextVelocity);
            float t = Mathf.Clamp01((angle - _forceCorrectionAngle.x) / (_forceCorrectionAngle.y - _forceCorrectionAngle.x));

            float f = Mathf.Lerp(_forceCorrection.x, _forceCorrection.y, t);
            force = Vector3.Lerp(force, perfectForce, f);
        }

        private void UpdateFriction(float slopeAngle) {
            var mat = _collider.material;
            
            if (_groundDetector.HasContact) {
                float absAngle = Mathf.Abs(slopeAngle);

                if (absAngle < _slopeAngle.x) {
                    mat.frictionCombine = _frictionCombineGrounded;
                    mat.dynamicFriction = _frictionGrounded;
                    mat.staticFriction = _frictionGrounded;
                    
                    return;
                }
                
                if (absAngle <= _slopeAngle.y) {
                    mat.frictionCombine = _frictionCombineSlope;
                    mat.dynamicFriction = _frictionSlope;
                    mat.staticFriction = _frictionSlope;
                    
                    return;
                }
                
                mat.frictionCombine = _frictionCombineSlopeOverMaxAngle;
                mat.dynamicFriction = _frictionSlopeOverMaxAngle;
                mat.staticFriction = _frictionSlopeOverMaxAngle;
                
                return;
            }
            
            mat.frictionCombine = _frictionCombineAir;
            mat.dynamicFriction = _frictionAir;
            mat.staticFriction = _frictionAir;
        }

        private float GetSlopeAngle(Vector3 inputDir) {
            var up = _rigidbody.transform.up;
            return Vector3.SignedAngle(
                up,
                _groundDetector.CollisionInfo.normal, 
                Vector3.Cross(inputDir, up).normalized
            );
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos() {
            if (!_showDebugInfo) return;

            if (Application.isPlaying) {
                UnityEditor.Handles.Label(
                    transform.TransformPoint(Vector3.up),
                    $"Speed {_rigidbody.velocity.magnitude:0.00} / {CalculateSpeedCorrection(_smoothedInput) * Speed:0.00}\n" +
                    $"Move force {_moveForce:0.00}"
                );
            }
        }
#endif
    }

}
