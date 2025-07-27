using System;
using MisterGames.Collisions.Rigidbodies;
using MisterGames.Common;
using MisterGames.Common.Attributes;
using MisterGames.Logic.Phys;
using UnityEngine;

namespace MisterGames.Logic.Water {
    
    [RequireComponent(typeof(BoxCollider))]
    public sealed class WaterZoneProxy : MonoBehaviour, IWaterZoneProxy {

        [SerializeField] private BoxCollider _waterBox;
        [SerializeField] private TriggerEmitter _triggerEmitter;
        
        [Header("Force")]
        [SerializeField] private float _surfaceOffset;
        [SerializeField] private float _force = 0f;
        [SerializeField] [Min(0f)] private float _forceLevelDecrease = 0f;
        [SerializeField] private ForceSource _forceSource;
        [VisibleIf(nameof(_forceSource), 1)]
        [SerializeField] private GravityProvider _gravityProvider;
        
        private enum ForceSource {
            Constant,
            UseGravityMagnitude,
        }
        
        private Transform _waterBoxTransform;
        private IWaterZone _waterZone;

        private void Awake() {
            _waterBoxTransform = _waterBox.transform;
        }

        private void OnEnable() {
            _triggerEmitter.TriggerEnter += HandleTriggerEnter;
            _triggerEmitter.TriggerExit += HandleTriggerExit;

            ApplyEnterForEnteredColliders();
        }

        private void OnDisable() {
            _triggerEmitter.TriggerEnter -= HandleTriggerEnter;
            _triggerEmitter.TriggerExit -= HandleTriggerExit;
        }
        
        public void BindZone(IWaterZone waterZone) {
            if (_waterZone == waterZone) return;
            
            _waterZone = waterZone;
            ApplyEnterForEnteredColliders();
        }

        public void UnbindZone(IWaterZone waterZone) {
            if (_waterZone == null || _waterZone != waterZone) return;

            ApplyExitForEnteredColliders();
            _waterZone = null;
        }

        private void HandleTriggerEnter(Collider collider) {
            _waterZone?.TriggerEnter(collider, this);
        }

        private void HandleTriggerExit(Collider collider) {
            _waterZone?.TriggerExit(collider, this);
        }

        private void ApplyEnterForEnteredColliders() {
            if (_waterZone == null) return;
            
            foreach (var collider in _triggerEmitter.EnteredColliders) {
                if (collider != null) _waterZone.TriggerEnter(collider, this);
            }
        }
        
        private void ApplyExitForEnteredColliders() {
            if (_waterZone == null) return;
            
            foreach (var collider in _triggerEmitter.EnteredColliders) {
                _waterZone.TriggerExit(collider, this);
            }
        }

        public void SampleSurface(Vector3 position, out Vector3 surfacePoint, out Vector3 normal, out Vector3 force) {
            normal = _waterBoxTransform.up;
            
            var surfaceCenter = _waterBox.bounds.center + 
                                normal * (0.5f * _waterBox.size.y * _waterBoxTransform.localScale.y + _surfaceOffset);
            
            surfacePoint = position + Vector3.Project(surfaceCenter - position, normal);

            // Position is above the surface
            if (Vector3.Dot(surfacePoint - position, normal) <= 0f) {
                force = default;
                return;
            }

            float forceMul = _forceLevelDecrease > 0f 
                ? Mathf.Clamp01(Vector3.Project(surfacePoint - position, normal).magnitude / _forceLevelDecrease) 
                : 1f;
            
            float forceMagnitude = _forceSource switch {
                ForceSource.Constant => _force,
                ForceSource.UseGravityMagnitude => _force * _gravityProvider.GravityMagnitude,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            force = forceMagnitude * forceMul * normal;
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;

        private void Reset() {
            _waterBox = GetComponent<BoxCollider>();
        }

        private void OnDrawGizmos() {
            if (!_showDebugInfo || _waterBox == null) return;

            if (_waterBoxTransform == null || _waterBoxTransform != _waterBox.transform) {
                _waterBoxTransform = _waterBox.transform;
            }

            var center = _waterBox.bounds.center;
            var rot = _waterBoxTransform.rotation;
            
            SampleSurface(center, out var surfacePoint, out var normal, out _);
            
            DebugExt.DrawSphere(center, 0.03f, Color.white, gizmo: true);
            DebugExt.DrawLine(center, surfacePoint, Color.white, gizmo: true);
            
            DebugExt.DrawCrossedPoint(surfacePoint, rot, Color.cyan, gizmo: true);
            DebugExt.DrawCrossedPoint(surfacePoint - normal * _forceLevelDecrease, rot, Color.magenta, radius: 0f, gizmo: true);
        }
#endif
    }
    
}