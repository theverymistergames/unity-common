using System;
using MisterGames.Common;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    [RequireComponent(typeof(Rigidbody))]
    public sealed class RigidbodyCustomGravity : MonoBehaviour, IUpdate {
        
        [SerializeField] private Rigidbody _rigidbody;
        
        [Header("Gravity")]
        [SerializeField] private Mode _gravityMode = Mode.CustomGlobalOrPhysics;
        [VisibleIf(nameof(_gravityMode), 3)]
        [SerializeField] private CustomGravitySource _localGravitySource;
        [SerializeField] private bool _useGravity = true;
        [SerializeField] private float _gravityScale = 1f;

        [Header("Sleeping")]
        [SerializeField] private bool _allowSleeping = true;
        [SerializeField] [Min(0f)] private float _velocityMin = 0.01f;
        [SerializeField] [Min(0f)] private float _sleepDelay = 1f;

        public enum Mode {
            Physics,
            CustomGlobalOrPhysics,
            CustomGlobal,
            CustomLocal,
        }
        
        public Mode GravityMode { get => _gravityMode; set => _gravityMode = value; }
        public bool UseGravity { get => _useGravity; set => _useGravity = value; }
        public Vector3 Gravity => GravityDirection * GravityMagnitude;
        public Vector3 GravityDirection { get; private set; } = Vector3.down;
        public float GravityMagnitude { get; private set; } = GravityMagnitudeDefault;
        public float GravityScale { get => _gravityScale; set => _gravityScale = value; }

        public const float GravityMagnitudeDefault = 9.81f;

        private Vector3 _lastGravity = Vector3.down * GravityMagnitudeDefault;
        private float _sleepTimer;

        private void OnEnable() {
            PlayerLoopStage.FixedUpdate.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            var gravity = UpdateGravity(_rigidbody.position);
            bool changed = NotifyGravityVector(gravity);
            
            // Explicitly set not to use gravity OR
            // using default Unity gravity OR
            // gravity not changed and rb is sleeping: do nothing.
            if (!_useGravity || _rigidbody.useGravity || 
                !changed && _rigidbody.IsSleeping()) 
            {
                _sleepTimer = 0f;
                return;
            }
            
            if (_allowSleeping && _rigidbody.linearVelocity.sqrMagnitude < _velocityMin * _velocityMin) {
                _sleepTimer += dt;
            }
            else {
                _sleepTimer = 0f;
            }

            if (_sleepTimer > _sleepDelay) return;

            _rigidbody.AddForce(gravity, ForceMode.Acceleration);
        }

        private Vector3 UpdateGravity(Vector3 position) {
            switch (_gravityMode) {
                case Mode.Physics:
                    _rigidbody.useGravity = _useGravity && _gravityScale.IsNearlyEqual(1f);
                    return Physics.gravity * _gravityScale;
                
                case Mode.CustomGlobalOrPhysics:
                    if (CustomGravity.Main.TryGetGlobalGravity(position, out var g)) {
                        _rigidbody.useGravity = false;
                        return g * _gravityScale;
                    }

                    _rigidbody.useGravity = _useGravity && _gravityScale.IsNearlyEqual(1f);
                    return Physics.gravity * _gravityScale;
                
                case Mode.CustomGlobal:
                    _rigidbody.useGravity = false;
                    return CustomGravity.Main.GetGlobalGravity(position) * _gravityScale;
                
                case Mode.CustomLocal:
                    _rigidbody.useGravity = false;
                    return _localGravitySource.GetGravity(position, out _) * _gravityScale;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private bool NotifyGravityVector(Vector3 gravity) {
            if (gravity == _lastGravity) return false;
            
            _lastGravity = gravity;

            // Do not change last direction if gravity is zero
            if (gravity == default) {
                GravityMagnitude = 0f;
            }
            else {
                GravityDirection = gravity.normalized;
                GravityMagnitude = gravity.magnitude;
            }

            return true;
        }
        
#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;
        
        private void Reset() {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void OnDrawGizmos() {
            if (!Application.isPlaying || !_showDebugInfo) return;

            var p = _rigidbody.position;
            
            DebugExt.DrawSphere(p, 0.05f, Color.white, gizmo: true);
            DebugExt.DrawRay(p, GravityDirection, Color.white, gizmo: true);
            DebugExt.DrawLabel(p + GravityDirection, $"G = {GravityMagnitude:0.000}");
        }
#endif
    }
    
}