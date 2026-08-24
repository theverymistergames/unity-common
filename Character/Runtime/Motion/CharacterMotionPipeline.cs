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
        public bool HasBeenTeleported { get; private set; }
        
        public Vector3 MotionDirWorld { get; private set; }
        public Vector3 MotionNormal { get; private set; }
        public Vector3 InputDirWorld { get; private set; }
        public Vector2 Input { get; private set; }
        public Vector3 Up => _transform.up;
        
        public Rigidbody Rigidbody { get; private set; }
        public Vector3 Velocity { get => Rigidbody.linearVelocity; set => Rigidbody.linearVelocity = value; }

        public float MoveForce { get => _moveForce; set => _moveForce = value; }
        public float Speed { get; set; }
        public float SpeedCorrectionBack { get => _speedCorrectionBack; set => _speedCorrectionBack = value; }
        public float SpeedCorrectionSide { get => _speedCorrectionSide; set => _speedCorrectionSide = value; }
        
        private readonly Dictionary<IMotionProcessor, int> _processorIndexMap = new();
        private readonly List<IMotionProcessor> _processorList = new();
        
        private Transform _transform;
        private CharacterGravity _characterGravity;
        private CharacterViewPipeline _view;
        private CharacterInputPipeline _input;
        private CharacterGroundDetector _groundDetector;
        private CharacterCollisionPipeline _collisionPipeline;
        private Vector2 _smoothedInput;

        void IActorComponent.OnAwake(IActor actor) {
            _transform = actor.Transform;
            
            _input = actor.GetComponent<CharacterInputPipeline>();
            Rigidbody = actor.GetComponent<Rigidbody>();
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
            Rigidbody.MovePosition(Rigidbody.position + delta);
        }
        
        public void AddForce(Vector3 force, ForceMode mode = ForceMode.Force) {
            Rigidbody.AddForce(force, mode);
        }

        public void Teleport(Vector3 position, Quaternion rotation, bool preserveVelocity = true) {
            _collisionPipeline.Block(this, blocked: true);
            
            var velocity = Rigidbody.linearVelocity;
            var angularVelocity = Rigidbody.angularVelocity;
            
            Rigidbody.Sleep();

            var t = Rigidbody.transform;
            var oldBodyRotation = t.rotation;
            var oldHeadRotation = _view.HeadRotation;
            
            var up = -_characterGravity.GravityDirection;
            var rotDelta = rotation * Quaternion.Inverse(oldBodyRotation);
            var flatRotDelta = Quaternion.FromToRotation(
                Vector3.ProjectOnPlane(oldBodyRotation * Vector3.forward, up),
                Vector3.ProjectOnPlane(rotation * Vector3.forward, up)
            );
            
            var headOffset = t.InverseTransformPoint(_view.HeadPosition);

            Rigidbody.position = position;
            Rigidbody.rotation = rotation;
            
            t.SetPositionAndRotation(position, flatRotDelta * oldBodyRotation);
            
            _view.HeadRotation = rotDelta * oldHeadRotation;
            _view.HeadPosition = t.TransformPoint(headOffset);

            _view.Detach();
            _view.StopLookAt();
            _view.ResetHorizontalClamp();
            _view.ResetVerticalClamp();
            _view.ResetSmoothing();
            
            _collisionPipeline.Block(this, blocked: false);
            
            Rigidbody.WakeUp();

            if (!Rigidbody.isKinematic) {
                Rigidbody.linearVelocity = preserveVelocity ? rotDelta * velocity : Vector3.zero;
                Rigidbody.angularVelocity = preserveVelocity ? angularVelocity : Vector3.zero;
            }
            
            OnTeleport.Invoke();
            
            HasBeenTeleported = true;
        }
        
        private void HandleMotionInput(Vector2 input) {
            ApplyProcessorsForInputVector(ref input);
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

            if (Rigidbody.isKinematic) {
                CleanupProcessors();
                return;
            }
            
            float inputSpeed = Speed;
            ApplyProcessorsForInputSpeed(ref inputSpeed, dt);
            
            float maxSpeed = CalculateSpeedCorrection(Input) * inputSpeed;
            var velocity = Rigidbody.linearVelocity;

            var inputDirNormalized = normalRot * orient * (_smoothedInput == Vector2.zero ? Vector3.forward : InputToLocal(_smoothedInput).normalized);
            var inputDirSmoothed = normalRot * orient * InputToLocal(_smoothedInput);
            
            var velocityProj = Vector3.Project(velocity, inputDirNormalized);
            var force = VectorUtils.ClampAcceleration(inputDirSmoothed * _moveForce, velocityProj, maxSpeed, dt);
            
            ApplyProcessorsForInputForce(ref force, inputDirNormalized * maxSpeed, dt);
            
            CleanupProcessors();
            
            Rigidbody.AddForce(force, ForceMode.Acceleration);
            
#if UNITY_EDITOR
            if (_showDebugInfo) DebugExt.DrawSphere(Rigidbody.position, 0.05f, Color.green);
            if (_showDebugInfo) DebugExt.DrawRay(Rigidbody.position, MotionDirWorld, Color.green);
            if (_showDebugInfo) DebugExt.DrawRay(Rigidbody.position, MotionNormal, Color.cyan);
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
        
        private void ApplyProcessorsForInputVector(ref Vector2 input) {
            int count = _processorList.Count;
            for (int i = 0; i < count; i++) {
                if (_processorList[i] is { } processor) {
                    processor.ProcessInputVector(ref input);
                }
            }
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
                    $"Speed {Rigidbody.linearVelocity.magnitude:0.00} / {CalculateSpeedCorrection(_smoothedInput) * Speed:0.00}\n" +
                    $"Move force {_moveForce:0.00}"
                );
            }
        }
#endif
    }

}
