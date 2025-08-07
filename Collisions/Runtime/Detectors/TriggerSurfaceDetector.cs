using System.Collections.Generic;
using MisterGames.Collisions.Core;
using MisterGames.Collisions.Rigidbodies;
using MisterGames.Common;
using MisterGames.Common.Labels;
using MisterGames.Common.Layers;
using UnityEngine;

namespace MisterGames.Collisions.Detectors {
    
    public sealed class TriggerSurfaceDetector : MaterialDetectorBase {
        
        [SerializeField] private Transform _rootTransform;
        [SerializeField] private TriggerEmitter _triggerEmitter;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private LabelValue _material;
        [SerializeField] [Min(0f)] private float _weightMin = 0.1f;
        [SerializeField] [Min(0f)] private float _weightMax = 1f;
        [SerializeField] private float _topPoint;
        [SerializeField] private float _lowerPoint;
        
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

            float submergeWeight = GetSubmergeWeightMax();

#if UNITY_EDITOR
            if (_showDebugInfo) DebugExt.DrawCircle(point, Quaternion.identity, 0.05f, Color.green, duration: 1f);
#endif

            if (submergeWeight <= 0f) return _materialList;
            
            float weight = Mathf.Lerp(_weightMin, _weightMax, submergeWeight);
            
            if (weight > 0f) _materialList.Add(new MaterialInfo(_material.GetValue(), weight));
            
            return _materialList;
        }

        private float GetSubmergeWeightMax() {
            if (_colliders.Count == 0) return 0f;

            GetRootPoints(out var lowerPoint, out var topPoint);
            float maxLevel = 0f;
            
            foreach (var collider in _colliders) {
                maxLevel = Mathf.Max(maxLevel, GetSubmergeWeight(collider, lowerPoint, topPoint));
            }
            
            return maxLevel;
        }

        private float GetSubmergeWeight(Collider collider, Vector3 lowerPoint, Vector3 upperPoint) {
            var up = _rootTransform.up;

            var closestLowerPoint = lowerPoint + up * Vector3.Dot(collider.ClosestPoint(lowerPoint) - lowerPoint, up);
            var closestUpperPoint = lowerPoint + up * Vector3.Dot(collider.ClosestPoint(upperPoint) - lowerPoint, up);

            return upperPoint == lowerPoint 
                ? Vector3.Dot(upperPoint - closestUpperPoint, up) >= 0f ? 0f : 1f 
                : Mathf.Clamp01((closestUpperPoint - closestLowerPoint).magnitude / (upperPoint - lowerPoint).magnitude);
        }
        
        private void GetRootPoints(out Vector3 lowerPoint, out Vector3 topPoint) {
            var pos = _rootTransform.position;
            var up = _rootTransform.up;

            lowerPoint = pos + up * _lowerPoint;
            topPoint = pos + up * _topPoint;
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;

        private void OnValidate() {
            if (_weightMax < _weightMin) _weightMax = _weightMin;
        }

        private void OnDrawGizmos() {
            if (!_showDebugInfo || _rootTransform == null) return;
            
            GetRootPoints(out var lowerPoint, out var topPoint);
            var rot = _rootTransform.rotation;
            
            DebugExt.DrawCircle(lowerPoint, rot, 0.05f, Color.cyan, gizmo: true);
            DebugExt.DrawCircle(topPoint, rot, 0.05f, Color.magenta, gizmo: true);
        }
#endif
    }
    
}