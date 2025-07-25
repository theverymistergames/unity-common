using System.Collections.Generic;
using MisterGames.Collisions.Core;
using MisterGames.Collisions.Rigidbodies;
using MisterGames.Common;
using MisterGames.Common.Labels;
using MisterGames.Common.Layers;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Collisions.Detectors {
    
    public sealed class TriggerSurfaceDetector : MaterialDetectorBase {
        
        [SerializeField] private TriggerEmitter _triggerEmitter;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private LabelValue _material;
        [SerializeField] [Min(0f)] private float _weightMin = 0.1f;
        [SerializeField] [Min(0f)] private float _weightMax = 1f;
        [SerializeField] [Min(0f)] private float _maxDistance = 0.5f;
        
        private readonly List<MaterialInfo> _materialList = new();
        private readonly HashSet<Collider> _colliders = new();

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
            _colliders.Remove(collider);
        }

        public override IReadOnlyList<MaterialInfo> GetMaterials(Vector3 point, Vector3 normal) {
            _materialList.Clear();
            
#if UNITY_EDITOR
            if (_showDebugInfo) DebugExt.DrawSphere(point, 0.01f, Color.yellow, duration: 1f);
#endif

            var topPoint = point;
            
            if (!TrySampleTopPoint(normal, ref topPoint)) {
                return _materialList;
            }

#if UNITY_EDITOR
            if (_showDebugInfo) {
                DebugExt.DrawCircle(point, Quaternion.identity, 0.05f, Color.green, duration: 1f);
                DebugExt.DrawRay(point, normal * 0.005f, Color.green, duration: 1f);
            }
#endif

            float mag = VectorUtils.SignedMagnitudeOfProject(topPoint - point, normal);
            float weight = mag > 0f 
                ? Mathf.Lerp(_weightMin, _weightMax, _maxDistance > 0f ? Mathf.Clamp01(mag / _maxDistance) : 1f) 
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