using System;
using MisterGames.Collisions.Utils;
using MisterGames.Common;
using MisterGames.Common.Attributes;
using MisterGames.Common.Tick;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Logic.Phys {
    
    public sealed class ParentToRaycastHit : MonoBehaviour, IUpdate {
    
        [Header("Launch")]
        [SerializeField] private Transform _target;
        [SerializeField] private Mode _mode;
        [SerializeField] private bool _retryInUpdateIfFailed = true;
        [SerializeField] private bool _continueOnEnableIfRequestedDetection = true;
        [SerializeField] private bool _ignoreChildColliders = true;
        
        [Header("Detection")]
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] [Min(1)] private int _maxHits = 3;
        [SerializeField] private Vector3 _rayOriginOffset;
        [SerializeField] private Vector3 _rayOriginRotationOffset;
        [SerializeField] [Min(0f)] private float _rayDistance = 1f;
        [SerializeField] [Min(0f)] private float _rayRadius;
        [SerializeField] private QueryTriggerInteraction _queryTriggerInteraction = QueryTriggerInteraction.Ignore;

        [Header("Parent")]
        [SerializeField] private SetPositionMode _positionMode = SetPositionMode.HitPoint;
        [SerializeField] private SetRotationMode _rotationMode = SetRotationMode.HitNormal;
        [SerializeField] private Vector3 _positionOffset;
        [SerializeField] private Vector3 _rotationOffset;
        [SerializeField] private bool _compensateRayOffset = true;

        private enum Mode {
            OnAwake,
            OnEnable,
            Manual,
        }
        
        private enum SetPositionMode {
            DontChange,
            HitPoint,
            ParentPosition,
        }
        
        private enum SetRotationMode {
            DontChange,
            HitNormal,
            ParentRotation,
        }
        
        private RaycastHit[] _hits;
        private bool _isRequestedDetection;
        
        private void Awake() {
            _hits = new RaycastHit[_maxHits];
            
            if (_mode == Mode.OnAwake) PerformDetection();
        }

        private void OnEnable() {
            if (_mode == Mode.OnEnable || _isRequestedDetection && _continueOnEnableIfRequestedDetection) {
                PerformDetection();
            }
        }

        private void OnDisable() {
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
        }

        public void PerformDetection() {
            PerformDetection(_retryInUpdateIfFailed);
        }
        
        private void PerformDetection(bool retryInUpdateIfFailed) {
            _isRequestedDetection = true;

            if (TryDetectAndSetParent() || !retryInUpdateIfFailed) {
                _isRequestedDetection = false;
                PlayerLoopStage.FixedUpdate.Unsubscribe(this);
                return;
            }
            
            PlayerLoopStage.FixedUpdate.Subscribe(this);
        }
        
        void IUpdate.OnUpdate(float dt) {
            if (!TryDetectAndSetParent()) return;

            _isRequestedDetection = false;
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
        }

        private bool TryDetectAndSetParent() {
            if (!TryDetect(out var parent, out var pos, out var rot)) return false;
            
            _target.SetPositionAndRotation(pos, rot);
            _target.SetParent(parent, worldPositionStays: true);
            
            return true;
        }
        
        private bool TryDetect(out Transform parent, out Vector3 pos, out Quaternion rot) {
            parent = null;
            _target.GetPositionAndRotation(out pos, out rot);
            
            var origin = pos + rot * _rayOriginOffset;
            var rotation = rot * Quaternion.Euler(_rayOriginRotationOffset);

            if (!Raycast(origin, rotation * Vector3.forward, out var hit)) return false;

#if UNITY_EDITOR
            if (_showDebugInfo) DebugExt.DrawPointer(hit.point, Color.green, size: 0.01f, duration: 1f);
#endif
            
            parent = hit.collider.transform;
            var hitPoint = hit.point;
            var hitRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(rot * Vector3.forward, hit.normal), hit.normal);
            
            if (_compensateRayOffset) {
                hitPoint -= rot * _rayOriginOffset;
            }
            
            var targetPos = _positionMode switch {
                SetPositionMode.DontChange => pos,
                SetPositionMode.HitPoint => hitPoint,
                SetPositionMode.ParentPosition => parent.position,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            var targetRot = _rotationMode switch {
                SetRotationMode.DontChange => rot,
                SetRotationMode.HitNormal => hitRotation,
                SetRotationMode.ParentRotation => parent.rotation,
                _ => throw new ArgumentOutOfRangeException()
            };

            pos = targetPos + targetRot * _positionOffset;
            rot = targetRot * Quaternion.Euler(_rotationOffset);
            
            return true;
        } 

        private bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hit) {
            int hitCount = Physics.SphereCastNonAlloc(origin, _rayRadius, direction, _hits, _rayDistance, _layerMask, _queryTriggerInteraction);
            return _hits
                .Filter(ref hitCount, this, (self, h) => self.IsValidHit(h))
                .TryGetMinimumDistanceHit(hitCount, out hit);
        }

        private bool IsValidHit(RaycastHit hit) {
            return !_ignoreChildColliders || !hit.collider.transform.IsChildOf(_target);
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;

        private Transform _debugParent;
        private Vector3 _debugHitPoint;
        private float _debugShowParentLabelTimer;
        
        private void Reset() {
            _target = transform;
        }

        private void OnValidate() {
            if (_hits == null || _hits.Length != _maxHits) _hits = new RaycastHit[_maxHits];
        }

        private void OnDrawGizmosSelected() {
            if (!_showDebugInfo || _target == null) return;
            
            _target.GetPositionAndRotation(out var pos, out var rot);
            
            var origin = pos + rot * _rayOriginOffset;
            var rotation = rot * Quaternion.Euler(_rayOriginRotationOffset);
            
            DebugExt.DrawSphere(origin, 0.01f, Color.yellow, gizmo: true);
            DebugExt.DrawSphereCast(origin, origin + rotation * Vector3.forward * _rayDistance, _rayRadius, Color.yellow, gizmo: true);

            float dt = TimeSources.deltaTime;
            _debugShowParentLabelTimer -= dt;

            if (_debugParent != null) DebugExt.DrawLabel(_debugHitPoint + Vector3.up * 0.05f, _debugParent.name);
            
            if (_debugShowParentLabelTimer < 0f) _debugParent = null;
        }

        [Button]
        private void TestRaycastParent() {
            if (_target == null) return;
            
            if (_hits == null || _hits.Length != _maxHits) _hits = new RaycastHit[_maxHits];

            if (!TryDetect(out var parent, out var pos, out var rot)) {
                _debugShowParentLabelTimer = 0f;
                _debugParent = null;
                return;
            }
            
            Undo.RecordObject(_target, $"{nameof(ParentToRaycastHit)}.{nameof(TestRaycastParent)}");
            _target.SetPositionAndRotation(pos, rot);
            EditorUtility.SetDirty(_target);

            _debugShowParentLabelTimer = 3f;
            _debugParent = parent;
        }
#endif
    }
    
}