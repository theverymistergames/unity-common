using MisterGames.Collisions.Rigidbodies;
using MisterGames.Common;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Logic.Water {
    
    [RequireComponent(typeof(BoxCollider))]
    public sealed class WaterZoneProxy : MonoBehaviour, IWaterZoneProxy {

        [SerializeField] private TriggerEmitter _triggerEmitter;
        [SerializeField] private BoxCollider _waterBox;
        [SerializeField] private float _surfaceOffset;

        public int VolumeId => GetHashCode();
        public float SurfaceOffset => _surfaceOffset;

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

        public Vector3 GetClosestPoint(Vector3 position) {
            return _waterBox.ClosestPoint(position);
        }

        public void SampleSurface(Vector3 position, out Vector3 surfacePoint, out Vector3 normal) {
            var center = _waterBox.bounds.center;
            normal = _waterBoxTransform.up;
            
            var sc = center + normal * (_waterBoxTransform.localScale.y * _waterBox.size.y * 0.5f);
            surfacePoint = position + Vector3.Project(sc - position, normal);
        }

        public void GetBox(out Vector3 position, out Quaternion rotation, out Vector3 size) {
            position = _waterBox.bounds.center;
            rotation = _waterBoxTransform.rotation;
            size = _waterBox.size.Multiply(_waterBoxTransform.localScale);
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
            var normal = _waterBoxTransform.up;
            
            var surfaceCenter = _waterBox.bounds.center + normal * (0.5f * _waterBox.size.y * _waterBoxTransform.localScale.y);
            var surfacePoint = center + Vector3.Project(surfaceCenter - center, normal) + _surfaceOffset * normal;
            
            DebugExt.DrawSphere(center, 0.03f, Color.white, gizmo: true);
            DebugExt.DrawLine(center, surfacePoint, Color.white, gizmo: true);
            
            DebugExt.DrawCrossedPoint(surfacePoint, rot, Color.cyan, gizmo: true);
        }
#endif
    }
    
}