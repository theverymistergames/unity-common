using System;
using System.Collections.Generic;
using MisterGames.Actors;
using MisterGames.Character.Phys;
using MisterGames.Character.Input;
using MisterGames.Character.View;
using MisterGames.Common;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Character.Motion {

    public sealed class CharacterMotionPipeline : MonoBehaviour, IActorComponent, IUpdate {

        [Header("Motion")]
        [SerializeField] [Min(0f)] private float _moveForce;
        [SerializeField] private float _speedCorrectionSide = 0.8f;
        [SerializeField] private float _speedCorrectionBack = 0.6f;
        [SerializeField] private float _inputSmoothing = 20f;
        
        public event Action OnTeleport = delegate { }; 
        
        public Vector3 MotionDirWorld { get; private set; }
        public Vector3 MotionNormal { get; private set; }
        public Vector3 InputDirWorld { get; private set; }
        public Vector2 Input { get; private set; }
        public Vector3 Up => _transform.up;
        
        public bool IsKinematic { get => _rigidbody.isKinematic; set => _rigidbody.isKinematic = value; }
        public Vector3 Velocity { get => _rigidbody.linearVelocity; set => _rigidbody.linearVelocity = value; }
        public Vector3 Position { get => _rigidbody.position; set => _rigidbody.position = value; }
        public bool HasBeenTeleported { get; private set; }

        public float MoveForce { get => _moveForce; set => _moveForce = value; }
        public float Speed { get; set; }
        public float SpeedCorrectionBack { get => _speedCorrectionBack; set => _speedCorrectionBack = value; }
        public float SpeedCorrectionSide { get => _speedCorrectionSide; set => _speedCorrectionSide = value; }
        
        private readonly Dictionary<IMotionProcessor, int> _processorIndexMap = new();
        private readonly List<IMotionProcessor> _processorList = new();
        
        private Transform _transform;
        private Rigidbody _rigidbody;
        private CharacterGravity _characterGravity;
        private CharacterViewPipeline _view;
        private CharacterInputPipeline _input;
        private CharacterGroundDetector _groundDetector;
        private CharacterCollisionPipeline _collisionPipeline;
        private Vector2 _smoothedInput;

        void IActorComponent.OnAwake(IActor actor) {
            _transform = actor.Transform;
            
            _input = actor.GetComponent<CharacterInputPipeline>();
            _rigidbody = actor.GetComponent<Rigidbody>();
            _characterGravity = actor.GetComponent<CharacterGravity>();
            _view = actor.GetComponent<CharacterViewPipeline>();
            _groundDetector = actor.GetComponent<CharacterGroundDetector>();
            _collisionPipeline = actor.GetComponent<CharacterCollisionPipeline>();
        }
        
        private void OnEnable() {
            _input.OnMotionVectorChanged += HandleMotionInput;
            PlayerLoopStage.FixedUpdate.Subscribe(this);
        }

        private void OnDisable() {
            _input.OnMotionVectorChanged -= HandleMotionInput;
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
            
            _collisionPipeline.Block(this, blocked: false);
        }

        public void AddProcessor(IMotionProcessor processor) {
            if (!_processorIndexMap.TryAdd(processor, _processorList.Count)) return;

            _processorList.Add(processor);
        }

        public void RemoveProcessor(IMotionProcessor processor) {
            if (!_processorIndexMap.Remove(processor, out int index)) return;

            _processorList[index] = null;
        }

        public void Move(Vector3 delta) {
            _rigidbody.MovePosition(_rigidbody.position + delta);
        }
        
        public void AddForce(Vector3 force, ForceMode mode = ForceMode.Force) {
            _rigidbody.AddForce(force, mode);
        }

        public void Teleport(Vector3 position, Quaternion rotation, bool preserveVelocity = true) {
            _collisionPipeline.Block(this, blocked: true);
            
            var velocity = _rigidbody.linearVelocity;
            var angularVelocity = _rigidbody.angularVelocity;
            
            _rigidbody.Sleep();

            var t = _rigidbody.transform;
            var oldBodyRotation = t.rotation;
            var oldHeadRotation = _view.HeadRotation;
            
            var up = -_characterGravity.GravityDirection;
            var rotDelta = rotation * Quaternion.Inverse(oldBodyRotation);
            var flatRotDelta = Quaternion.FromToRotation(
                Vector3.ProjectOnPlane(oldBodyRotation * Vector3.forward, up),
                Vector3.ProjectOnPlane(rotation * Vector3.forward, up)
            );
            
            var headOffset = t.InverseTransformPoint(_view.HeadSmoothPosition);
            
            t.SetPositionAndRotation(position, flatRotDelta * oldBodyRotation);
            
            _view.HeadRotation = rotDelta * oldHeadRotation;
            _view.HeadSmoothPosition = t.TransformPoint(headOffset);

            _view.Detach();
            _view.StopLookAt();
            _view.ResetHorizontalClamp();
            _view.ResetVerticalClamp();
            _view.ResetSmoothing();
            _view.ResetSensitivity();
            _view.ResetHeadOffset();
            
            _collisionPipeline.Block(this, blocked: false);
            
            _rigidbody.WakeUp();

            if (!_rigidbody.isKinematic) {
                _rigidbody.linearVelocity = preserveVelocity ? rotDelta * velocity : Vector3.zero;
                _rigidbody.angularVelocity = preserveVelocity ? angularVelocity : Vector3.zero;
            }
            
            OnTeleport.Invoke();
            
            HasBeenTeleported = true;
        }
        
        private void HandleMotionInput(Vector2 input) {
            Input = input;
        }

        void IUpdate.OnUpdate(float dt) {
            var up = _transform.up;
            var orient = _view.HeadRotation;

            ApplyProcessorsForOrientation(ref orient);

            InputDirWorld = Input == Vector2.zero ? Vector3.zero : orient * InputToLocal(Input).normalized;
            MotionNormal = _groundDetector.GetMotionNormal(InputDirWorld);
            var normalRot = Quaternion.FromToRotation(up, MotionNormal);
            
            MotionDirWorld = normalRot * InputDirWorld;
            _smoothedInput = _smoothedInput.SmoothExpNonZero(Input, _inputSmoothing, dt);

            if (_rigidbody.isKinematic) {
                CleanupProcessors();
                return;
            }
            
            float inputSpeed = Speed;
            ApplyProcessorsForInputSpeed(ref inputSpeed, dt);
            
            float maxSpeed = CalculateSpeedCorrection(Input) * inputSpeed;
            var velocity = _rigidbody.linearVelocity;

            var inputDirNormalized = normalRot * orient * (_smoothedInput == Vector2.zero ? Vector3.forward : InputToLocal(_smoothedInput).normalized);
            var inputDirSmoothed = normalRot * orient * InputToLocal(_smoothedInput);
            
            var velocityProj = Vector3.Project(velocity, inputDirNormalized);
            var force = VectorUtils.ClampAcceleration(inputDirSmoothed * _moveForce, velocityProj, maxSpeed, dt);
            
            ApplyProcessorsForInputForce(ref force, inputDirNormalized * maxSpeed, dt);
            
            CleanupProcessors();
            
            _rigidbody.AddForce(force, ForceMode.Acceleration);
            
#if UNITY_EDITOR
            if (_showDebugInfo) DebugExt.DrawSphere(_rigidbody.position, 0.05f, Color.green);
            if (_showDebugInfo) DebugExt.DrawRay(_rigidbody.position, MotionDirWorld, Color.green);
            if (_showDebugInfo) DebugExt.DrawRay(_rigidbody.position, MotionNormal, Color.cyan);
#endif
        }

        private static Vector3 InputToLocal(Vector2 input) {
            return new Vector3(input.x, 0f, input.y);
        }

        private float CalculateSpeedCorrection(Vector2 input) {
            // Moving backwards OR backwards + sideways: apply back correction
            if (input.y < 0) return _speedCorrectionBack;

            // Moving forwards OR forwards + sideways: no adjustment
            if (input.y > 0) return 1f;

            // Moving sideways only: apply side correction
            return _speedCorrectionSide;
        }
        
        private void ApplyProcessorsForOrientation(ref Quaternion orientation) {
            int count = _processorList.Count;
            int topPriority = int.MinValue;
            
            for (int i = 0; i < count; i++) {
                if (_processorList[i] is not { } processor) continue;

                var orientationProcessed = orientation;
                if (!processor.ProcessOrientation(ref orientationProcessed, out int priority) || priority < topPriority) continue;
                
                topPriority = priority;
                orientation = orientationProcessed;
            }
        }
        
        private void ApplyProcessorsForInputSpeed(ref float inputSpeed, float dt) {
            int count = _processorList.Count;
            for (int i = 0; i < count; i++) {
                if (_processorList[i] is { } processor) {
                    processor.ProcessInputSpeed(ref inputSpeed, dt);
                }
            }
        }
        
        private void ApplyProcessorsForInputForce(ref Vector3 inputForce, Vector3 desiredVelocity, float dt) {
            int count = _processorList.Count;
            for (int i = 0; i < count; i++) {
                if (_processorList[i] is { } processor) {
                    processor.ProcessInputForce(ref inputForce, desiredVelocity, dt);
                }
            }
        }

        private void CleanupProcessors() {
            int count = _processorList.Count;
            int validCount = count;
            
            for (int i = count - 1; i >= 0; i--) {
                if (_processorList[i] is null && _processorList[--validCount] is { } swap) {
                    _processorList[i] = swap;
                    _processorIndexMap[swap] = i;
                }
            }
            
            _processorList.RemoveRange(validCount, count - validCount);
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;
        
        private void OnDrawGizmos() {
            if (!_showDebugInfo) return;

            if (Application.isPlaying) {
                Handles.Label(
                    transform.TransformPoint(Vector3.up),
                    $"Speed {_rigidbody.linearVelocity.magnitude:0.00} / {CalculateSpeedCorrection(_smoothedInput) * Speed:0.00}\n" +
                    $"Move force {_moveForce:0.00}"
                );
            }
        }
#endif
    }

}
