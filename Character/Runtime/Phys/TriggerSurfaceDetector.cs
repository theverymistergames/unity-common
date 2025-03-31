using System.Collections.Generic;
using MisterGames.Collisions.Rigidbodies;
using MisterGames.Common;
using MisterGames.Common.Labels;
using MisterGames.Common.Layers;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Phys {
    
    public sealed class TriggerSurfaceDetector : MaterialDetectorBase {
        
        [SerializeField] private TriggerEmitter _triggerEmitter;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private CapsuleCollider _capsuleCollider;
        [SerializeField] private LabelValue _material;
        [SerializeField] [Min(0f)] private float _weightMin = 0.1f;
        [SerializeField] [Min(0f)] private float _weightMax = 1f;
        [SerializeField] private float _topPointOffset;
        
        private readonly List<MaterialInfo> _materialList = new();
        private readonly HashSet<Collider> _colliders = new();
        private Transform _transform;
        
        private void Awake() {
            _transform = _capsuleCollider.transform;
        }

        private void OnEnable() {
            _triggerEmitter.TriggerEnter += TriggerEnter;
            _triggerEmitter.TriggerExit += TriggerExit;
        }

        private void OnDisable() {
            _triggerEmitter.TriggerEnter -= TriggerEnter;
            _triggerEmitter.TriggerExit -= TriggerExit;
        }

        private void TriggerEnter(Collider collider) {
            if (!_layerMask.Contains(collider.gameObject.layer)) return;
            
            _colliders.Add(collider);
        }

        private void TriggerExit(Collider collider) {
            if (!_layerMask.Contains(collider.gameObject.layer)) return;
            
            _colliders.Remove(collider);
        }

        public override IReadOnlyList<MaterialInfo> GetMaterials() {
            _materialList.Clear();
            
            var up = _transform.up;
            float halfHeight = _capsuleCollider.height * 0.5f;
            
            var lowPoint = _transform.TransformPoint(_capsuleCollider.center) - halfHeight * up;
            var point = lowPoint;
            
#if UNITY_EDITOR
            if (_showDebugInfo) DebugExt.DrawSphere(point, 0.01f, Color.yellow, duration: 1f);
#endif

            if (!TrySampleTopPoint(up, ref point)) {
                return _materialList;
            }

#if UNITY_EDITOR
            if (_showDebugInfo) {
                DebugExt.DrawCircle(point, _transform.rotation, 0.05f, Color.green, duration: 1f);
                DebugExt.DrawRay(point, _transform.up * 0.005f, Color.green, duration: 1f);
            }
#endif

            float mag = VectorUtils.SignedMagnitudeOfProject(point - lowPoint, up);
            float diff = halfHeight + _topPointOffset;  
            float weight = mag > 0f 
                ? Mathf.Lerp(_weightMin, _weightMax, diff > 0f ? Mathf.Clamp01(mag / diff) : 1f) 
                : 0f;

            if (weight > 0f) _materialList.Add(new MaterialInfo(_material.GetValue(), weight));
            
            return _materialList;
        }

        private bool TrySampleTopPoint(Vector3 up, ref Vector3 point) {
            if (_colliders.Count == 0) return false;
            
            float maxDistance = float.MinValue;
            var topPoint = point;
            
            foreach (var collider in _colliders) {
                var pos = collider.transform.position;
                var proj = Vector3.Project(pos - point, up);

                float dist = proj.magnitude;
                if (dist < maxDistance) continue;

                topPoint = point + proj;
                maxDistance = dist;
            }

            point = topPoint;
            return true;
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;

        private void OnValidate() {
            if (_weightMax < _weightMin) _weightMax = _weightMin;
        }
#endif
    }
    
}