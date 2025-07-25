﻿using System;
using System.Collections.Generic;
using MisterGames.Common;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.Logic.Phys {
    
    [DefaultExecutionOrder(-10_000)]
    public sealed class RigidbodyCustomGravityGroup : MonoBehaviour, IUpdate {
        
        [Header("Default State")]
        [SerializeField] private Options _options;
        [SerializeField] private StartMode _startMode;
        
        [Header("Gravity")]
        [SerializeField] private Transform _center;
        [SerializeField] private RigidbodyCustomGravity.Mode _gravityMode = RigidbodyCustomGravity.Mode.CustomGlobalOrPhysics;
        [VisibleIf(nameof(_gravityMode), 3)]
        [SerializeField] private CustomGravitySource _localGravitySource;
        [SerializeField] private bool _useGravity = true;
        [SerializeField] private float _gravityScale = 1f;
        
        [Header("Torque")]
        [SerializeField] private bool _addRandomTorqueOnGravityChange = true;
        [SerializeField] [Min(0f)] private float _torqueMin = 0f;
        [SerializeField] [Min(0f)] private float _torqueMax = 60f;
        
        [Flags]
        public enum Options {
            UseDefault = 0,
            None = 1,
            SetKinematic = 2,
            ReturnToInitialPosition = 4,
            ContinuousSpeculativeCCD = 8,
        }
        
        private enum StartMode {
            OnEnable,
            OnGravityChanged,
        }

        private readonly struct RigidbodyData {
            public readonly Vector3 position;
            public readonly Quaternion rotation;
            public readonly Options options;
            
            public RigidbodyData(Vector3 position, Quaternion rotation, Options options) {
                this.position = position;
                this.rotation = rotation;
                this.options = options;
            }
        }
        
        public RigidbodyCustomGravity.Mode GravityMode { get => _gravityMode; set => _gravityMode = value; }
        public bool UseGravity { get => _useGravity; set => _useGravity = value; }
        public Vector3 Gravity => GravityDirection * GravityMagnitude;
        public Vector3 GravityDirection { get; private set; } = Vector3.down;
        public float GravityMagnitude { get; private set; } = GravityMagnitudeDefault;
        public float GravityScale { get => _gravityScale; set => _gravityScale = value; }

        private const float GravityMagnitudeDefault = 9.81f;

        private readonly Dictionary<Rigidbody, RigidbodyData> _rigidbodyMap = new();
        private Vector3 _lastGravity = Vector3.down * GravityMagnitudeDefault;
        private bool _isCustomGravityActive;

        private void OnDestroy() {
            _rigidbodyMap.Clear();
        }

        private void OnEnable() {
            PlayerLoopStage.FixedUpdate.Subscribe(this);

            if (_startMode != StartMode.OnEnable) return;
            
            _isCustomGravityActive = true;
            
            foreach (var rb in _rigidbodyMap.Keys) {
                SetupRigidbodyActiveState(rb);
            }
        }

        private void OnDisable() {
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
            
            _isCustomGravityActive = false;
            _lastGravity = Vector3.down * GravityMagnitudeDefault;
            
            foreach (var rb in _rigidbodyMap.Keys) {
                SetupRigidbodyInitialState(rb);
            }
        }

        public void Register(Rigidbody rigidbody, Options options) {
            if (!_rigidbodyMap.TryAdd(rigidbody, new RigidbodyData(rigidbody.position, rigidbody.rotation, CreateOptions(options, _options)))) return;
            
            if (_isCustomGravityActive) SetupRigidbodyActiveState(rigidbody);
            else SetupRigidbodyInitialState(rigidbody);
        }

        public void Unregister(Rigidbody rigidbody) {
            SetupRigidbodyInitialState(rigidbody);
            _rigidbodyMap.Remove(rigidbody);
        }

        private static Options CreateOptions(Options optionsOfRigidbody, Options defaultOptions) {
            return optionsOfRigidbody == Options.UseDefault ? defaultOptions : optionsOfRigidbody;
        }

        private void SetupRigidbodyActiveState(Rigidbody rigidbody) {
            if (!_rigidbodyMap.TryGetValue(rigidbody, out var data)) return;
            
            rigidbody.isKinematic = false;
            
            if ((data.options & Options.ContinuousSpeculativeCCD) == Options.ContinuousSpeculativeCCD) {
                rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            }
        }

        private void SetupRigidbodyInitialState(Rigidbody rigidbody) {
            if (!_rigidbodyMap.TryGetValue(rigidbody, out var data) || rigidbody == null) return;

            if ((data.options & Options.SetKinematic) == Options.SetKinematic) {
                rigidbody.isKinematic = true;
            }

            if ((data.options & Options.ReturnToInitialPosition) == Options.ReturnToInitialPosition) {
                rigidbody.Sleep();
            
                rigidbody.position = data.position;
                rigidbody.rotation = data.rotation;
            
                rigidbody.WakeUp();   
            }
        }
        
        void IUpdate.OnUpdate(float dt) {
            var lastGravity = _lastGravity;
            var gravity = GetGravity(_center.position, out bool useGravity);
            bool changed = NotifyGravityVector(gravity);
            
            if (!_isCustomGravityActive) {
                if (!changed) return;
                
                _isCustomGravityActive = true;
                
                foreach (var rb in _rigidbodyMap.Keys) {
                    SetupRigidbodyActiveState(rb);
                }
            }

            if (changed && Vector3.Dot(gravity, lastGravity) < 0f) {
                foreach (var rb in _rigidbodyMap.Keys) {
                    rb.AddTorque(Random.Range(_torqueMin, _torqueMax) * Random.onUnitSphere, ForceMode.Acceleration);
                }
            }
            
            foreach (var rb in _rigidbodyMap.Keys) {
                rb.useGravity = useGravity;
                
                if (!_useGravity || !changed && rb.IsSleeping()) return;
                
                rb.AddForce(gravity, ForceMode.Acceleration);
            }
        }

        private Vector3 GetGravity(Vector3 position, out bool useGravity) {
            switch (_gravityMode) {
                case RigidbodyCustomGravity.Mode.Physics:
                    useGravity = _useGravity && _gravityScale.IsNearlyEqual(1f);
                    return Physics.gravity * _gravityScale;
                
                case RigidbodyCustomGravity.Mode.CustomGlobalOrPhysics:
                    if (CustomGravity.Main.TryGetGlobalGravity(position, out var g)) {
                        useGravity = false;
                        return g * _gravityScale;
                    }

                    useGravity = _useGravity && _gravityScale.IsNearlyEqual(1f);
                    return Physics.gravity * _gravityScale;
                
                case RigidbodyCustomGravity.Mode.CustomGlobal:
                    useGravity = false;
                    return CustomGravity.Main.GetGlobalGravity(position) * _gravityScale;
                
                case RigidbodyCustomGravity.Mode.CustomLocal:
                    useGravity = false;
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
        
        private void OnDrawGizmos() {
            if (!Application.isPlaying || !_showDebugInfo) return;

            foreach (var rb in _rigidbodyMap.Keys) {
                var p = rb.position;
            
                DebugExt.DrawSphere(p, 0.05f, Color.white, gizmo: true);
                
                if (!_isCustomGravityActive) continue;
                
                DebugExt.DrawRay(p, GravityDirection, Color.white, gizmo: true);
                DebugExt.DrawLabel(p + GravityDirection, $"G = {GravityMagnitude:0.000}");   
            }
        }
#endif
    }
    
}