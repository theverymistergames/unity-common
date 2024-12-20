using System;
using MisterGames.Collisions.Core;
using MisterGames.Collisions.Utils;
using MisterGames.Common;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Collisions {

    public sealed class CharacterGroundDetector : CollisionDetectorBase, IRadiusCollisionDetector, IUpdate {

        [Header("Spherecast")]
        [SerializeField] [Min(1)] private int _maxHits = 6;
        [SerializeField] private float _maxValidDotProduct = -0.001f;
        [SerializeField] private float _originPointLevel;
        [SerializeField] private float _distance = 0.55f;
        [SerializeField] private float _distanceAddition = 0.15f;
        [SerializeField] private float _radius = 0.3f;
        [SerializeField] private float _secondaryRadiusOffset = -0.01f;
        [SerializeField] private LayerMask _layerMask;

        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;
        [SerializeField] [Min(0f)] private float _traceDuration = 3f;
        
        public override int Capacity => _maxHits;
        public override Vector3 OriginOffset { get; set; }
        public override float Distance { get => _distance; set => _distance = value; }
        public float Radius { get => _radius; set => _radius = value; }

        private Transform _transform;
        private CollisionInfo[] _hitsMain;
        private RaycastHit[] _raycastHitsMain;
        private int _hitCount;
        
        private void Awake() {
            _transform = transform;
            _raycastHitsMain = new RaycastHit[_maxHits];
        }

        private void OnEnable() {
            PlayerLoopStage.FixedUpdate.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
        }

        private void Start() {
            RequestGround(forceNotify: true);
        }

        void IUpdate.OnUpdate(float dt) {
            RequestGround();
        }

        public override ReadOnlySpan<CollisionInfo> FilterLastResults(CollisionFilter filter) {
            int hitCount = _hitCount;
            
            _raycastHitsMain
                .RemoveInvalidHits(ref hitCount)
                .Filter(ref hitCount, filter);

            if (hitCount <= 0) return ReadOnlySpan<CollisionInfo>.Empty;

            for (int i = 0; i < hitCount; i++) {
                _hitsMain[i] = CollisionInfo.FromRaycastHit(_raycastHitsMain[i]);
            }

            return ((ReadOnlySpan<CollisionInfo>) _hitsMain)[..hitCount];
        }

        public Vector3 GetMotionNormal(Vector3 desiredMotion) {
            var up = _transform.up;
            var origin = _transform.TransformPoint(OriginOffset);
            var lowerCenter = origin - up * _distance;
            var normal = up;
            
            // Trying to find contact points placed in front of current position
            // and calculate their average normal
            for (int i = 0; i < _hitCount; i++) {
                var hit = _raycastHitsMain[i];
                
                // Filter out contacts behind current position.
                if (Vector3.Dot(hit.point - lowerCenter, desiredMotion) < 0f) continue;

                normal += hit.normal;
            }

            return normal.normalized;
        }

        private void RequestGround(bool forceNotify = false) {
            if (!enabled) return;

            var up = _transform.up;
            var pos = _transform.TransformPoint(OriginOffset);
            
            var origin = pos + up * _originPointLevel;
            float distance = _distance + _distanceAddition + _originPointLevel;
            var lowerCenter = pos - up * _distance;
            
            _hitCount = DetectGround(origin, lowerCenter, _radius, distance, _raycastHitsMain);
            
            var normal = Vector3.zero;
            var hitPoint = Vector3.zero;
            float hitDistance;
            Transform surface = null;

            if (_hitCount > 0) {
                float minSqrMagnitude = -1f;

                for (int i = 0; i < _hitCount; i++) {
                    var hit = _raycastHitsMain[i];
                    float sqrMagnitude = (origin - hit.point).sqrMagnitude;
                    
                    if (sqrMagnitude < minSqrMagnitude || minSqrMagnitude < 0f) {
                        minSqrMagnitude = sqrMagnitude;
                        surface = hit.transform;
                        hitPoint = hit.point;
                    }

                    normal += hit.normal;
                }

                normal = normal.normalized;
                hitDistance = Vector3.Distance(origin, hitPoint);
            }
            else {
                normal = up;
                hitPoint = origin - distance * up;
                hitDistance = distance;
            }
            
#if UNITY_EDITOR
            if (_showDebugInfo) DebugExt.DrawRay(hitPoint, normal, Color.blue);
            if (_showDebugInfo && _hitCount > 0) DebugExt.DrawPointer(hitPoint, Color.yellow, 0.05f);
            if (_traceDuration > 0f) {
                var p = lowerCenter - up * _radius;
                DebugExt.DrawSphere(p, 0.005f, Color.yellow, duration: _traceDuration);
                DebugExt.DrawLine(_lastBottomPoint, p, Color.yellow, duration: _traceDuration);
                DebugExt.DrawRay(p, normal * 0.03f, Color.cyan, duration: _traceDuration);
                _lastBottomPoint = p;
            }
#endif

            var info = new CollisionInfo(_hitCount > 0, hitDistance, normal, hitPoint, surface);
            SetCollisionInfo(info, forceNotify);
        }

        private int DetectGround(Vector3 origin, Vector3 lowerCenter, float radius, float distance, RaycastHit[] hits) {
            var up = _transform.up;
            int hitCount = Physics.SphereCastNonAlloc(origin, radius, -up, hits, distance, _layerMask, QueryTriggerInteraction.Ignore);

#if UNITY_EDITOR
            if (_showDebugInfo) {
                DebugExt.DrawSphereCast(origin, origin - up * distance, radius, Color.yellow);
                for (int i = 0; i < hitCount; i++) {
                    DebugExt.DrawPointer(hits[i].point, IsValidGroundHit(hits[i], lowerCenter, up) ? Color.green : Color.magenta, 0.03f);
                }
            }
#endif
            
            hits.Filter(
                ref hitCount, 
                data: (self: this, lowerCenter, up), 
                predicate: (data, hit) => data.self.IsValidGroundHit(hit, data.lowerCenter, data.up)
            );

            // Second cast to avoid case when main spherecast fails to detect ground:
            // this may happen if radius of cast equals character capsule radius.
            if (hitCount <= 0) {
                hitCount = Physics.SphereCastNonAlloc(origin, radius + _secondaryRadiusOffset, -up, hits, distance, _layerMask, QueryTriggerInteraction.Ignore);
                
#if UNITY_EDITOR
                if (_showDebugInfo) {
                    DebugExt.DrawSphereCast(origin, origin - up * distance, radius + _secondaryRadiusOffset, Color.white);
                    for (int i = 0; i < hitCount; i++) {
                        DebugExt.DrawPointer(hits[i].point, IsValidGroundHit(hits[i], lowerCenter, up) ? Color.green : Color.magenta, 0.03f);
                    }
                }
#endif          
                
                hits.Filter(
                    ref hitCount, 
                    data: (self: this, lowerCenter, up), 
                    predicate: (data, hit) => data.self.IsValidGroundHit(hit, data.lowerCenter, data.up)
                );
            }
            
            return hitCount;
        }

        private bool IsValidGroundHit(RaycastHit hit, Vector3 lowerCenter, Vector3 up) {
            return hit.distance > 0f &&
                   Vector3.Dot(hit.point - lowerCenter, up) < _maxValidDotProduct &&
                   Vector3.ProjectOnPlane(hit.point - lowerCenter, up).sqrMagnitude <= _radius * _radius;
        }

#if UNITY_EDITOR
        private Vector3 _lastBottomPoint;
        private void OnDrawGizmos() {
            if (!_showDebugInfo) return;

            var t = transform;
            var r = t.right;
            var up = t.up;
            var lowerPoint = t.TransformPoint(OriginOffset) - up * _distance;

            DebugExt.DrawSphere(lowerPoint, 0.02f, Color.yellow, gizmo: true);
            DebugExt.DrawLine(lowerPoint + r * 0.2f, lowerPoint - r * 0.2f, Color.yellow, gizmo: true);
            
            if (!Application.isPlaying) {
                var origin = transform.TransformPoint(OriginOffset) + up * _originPointLevel;
                float dist = _distance + _distanceAddition + _originPointLevel;
                
                DebugExt.DrawSphereCast(origin, origin - up * dist, _radius, Color.yellow, gizmo: true);
                DebugExt.DrawSphereCast(origin, origin - up * dist, _radius + _secondaryRadiusOffset, Color.white, gizmo: true);
            }

            if (Application.isPlaying) {
                string text = CollisionInfo.hasContact ? "grounded" : "in air";
                DebugExt.DrawLabel(transform.TransformPoint(OriginOffset + Vector3.up), text);
            }
        }
#endif
    }

}
