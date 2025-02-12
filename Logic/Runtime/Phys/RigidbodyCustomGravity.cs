using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    [RequireComponent(typeof(Rigidbody))]
    public sealed class RigidbodyCustomGravity : MonoBehaviour, IUpdate {
        
        [SerializeField] private Rigidbody _rigidbody;
        
        [Header("Gravity")]
        [SerializeField] private GravityMode _gravityMode;
        [VisibleIf(nameof(_gravityMode), 2)]
        [SerializeField] private CustomGravitySource _localGravitySource;
        [SerializeField] private bool _useGravity;
        [SerializeField] private float _gravityScale = 1f;

        [Header("Sleeping")]
        [SerializeField] private bool _allowSleeping = true;
        [SerializeField] [Min(0f)] private float _velocityMin = 0.01f;
        [SerializeField] [Min(0f)] private float _sleepDelay = 1f;

        public GravityMode GravityMode {
            get => _gravityMode;
            set {
                _gravityMode = value;
                UpdateGravityUsage();
            }
        }

        public bool UseGravity {
            get => _useGravity;
            set {
                _useGravity = value;
                UpdateGravityUsage();
            }
        }

        public float GravityScale {
            get => _gravityScale;
            set {
                _gravityScale = value;
                UpdateGravityUsage();
            }
        }

        public Vector3 Gravity => GravityDirection * GravityMagnitude;
        public Vector3 GravityDirection { get; private set; } = Vector3.down;
        public float GravityMagnitude { get; private set; } = GravityMagnitudeDefault;
        
        private const float GravityMagnitudeDefault = 9.81f;

        private Vector3 _lastGravity = Vector3.down * GravityMagnitudeDefault;
        private float _sleepTimer;

        private void OnEnable() {
            UpdateGravityUsage();
            PlayerLoopStage.FixedUpdate.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            var gravity = GetGravity(_rigidbody.position) * _gravityScale;
            UpdateGravityVector(gravity);

            if (_rigidbody.useGravity) {
                _sleepTimer = 0f;
                return;
            }
            
            if (_rigidbody.IsSleeping()) {
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

        private void UpdateGravityVector(Vector3 gravity) {
            if (gravity == _lastGravity) return;
            
            _lastGravity = gravity;

            // Do not change last direction if gravity is zero
            if (gravity == default) {
                GravityMagnitude = 0f;
                return;
            }
            
            GravityDirection = gravity.normalized;
            GravityMagnitude = gravity.magnitude;
        }
        
        private void UpdateGravityUsage() {
            _rigidbody.useGravity = _useGravity && 
                                    _gravityMode == GravityMode.Physics && 
                                    _gravityScale.IsNearlyEqual(1f);
        }

        private Vector3 GetGravity(Vector3 position) {
            return _gravityMode switch {
                GravityMode.Physics => Physics.gravity,
                GravityMode.CustomGlobal => CustomGravity.Main.GetGlobalGravity(position),
                GravityMode.CustomLocal => _localGravitySource.GetGravity(position, out _),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
#if UNITY_EDITOR
        private void Reset() {
            _rigidbody = GetComponent<Rigidbody>();
        }  
#endif
    }
    
}