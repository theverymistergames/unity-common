using System;
using MisterGames.Collisions.Core;
using MisterGames.Collisions.Utils;
using MisterGames.Common;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Collisions {

    public class CharacterGroundDetector : CollisionDetectorBase, IRadiusCollisionDetector, IUpdate {

        [SerializeField] private PlayerLoopStage _timeSourceStage = PlayerLoopStage.Update;

        [Header("Spherecast")]
        [SerializeField] [Min(1)] private int _maxHits = 6;
        [SerializeField] private float _distance = 0.55f;
        [SerializeField] private float _distanceAddition = 0.15f;
        [SerializeField] private float _radius = 0.3f;
        [SerializeField] private LayerMask _layerMask;

        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;
        [SerializeField] private float _bottomPoint;
        [SerializeField] [Min(0f)] private float _traceDuration = 3f;
        
        public override int Capacity => _maxHits;

        public override Vector3 OriginOffset {
            get => _originOffset;
            set {
                _originOffset = value;
                _invalidateFlag = true;
            }
        }

        public override float Distance {
            get => _distance;
            set {
                _distance = value;
                _invalidateFlag = true;
            }
        }

        public float Radius {
            get => _radius;
            set {
                _radius = value;
                _invalidateFlag = true;
            }
        }
        
        public Vector3 Forward {
            get => _forward;
            set {
                _forward = value;
                _invalidateFlag = true;
            }
        }

        private Transform _transform;

        private CollisionInfo[] _hitsMain;
        private RaycastHit[] _raycastHitsMain;
        private int _hitCount;

        private Vector3 _originOffset;
        private Vector3 _forward;
        private int _lastUpdateFrame;
        private bool _invalidateFlag;

        private void Awake() {
            _transform = transform;
            _raycastHitsMain = new RaycastHit[_maxHits];
        }

        private void OnEnable() {
            _timeSourceStage.Subscribe(this);
        }

        private void OnDisable() {
            _timeSourceStage.Unsubscribe(this);
        }

        private void Start() {
            RequestGround(forceNotify: true);
        }

        public void OnUpdate(float dt) {
            RequestGround();
        }

        public override void FetchResults() {
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

        private void RequestGround(bool forceNotify = false) {
            int frame = Time.frameCount;
            if (!enabled || frame == _lastUpdateFrame && !_invalidateFlag) return;

            var up = _transform.up;
            var origin = _transform.TransformPoint(_originOffset);
            float distance = _distance + _distanceAddition;
            
            _hitCount = DetectGround(origin, _radius, distance, _raycastHitsMain);
            
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

                if (_forward.sqrMagnitude > 0f && 
                    TryDetectMotionNormal(origin, _forward, out var motionNormal)
                ) {
                    normal = motionNormal;
                }
            }
            else {
                normal = up;
                hitPoint = origin - distance * up;
                hitDistance = distance;
            }
            
#if UNITY_EDITOR
            if (_showDebugInfo) DebugExt.DrawRay(hitPoint, normal, Color.blue);
            if (_showDebugInfo && _hitCount > 0) DebugExt.DrawPointer(hitPoint, Color.yellow, 0.3f);
            if (_traceDuration > 0f) {
                var p = _transform.TransformPoint(Vector3.up * _bottomPoint);
                DebugExt.DrawSphere(p, 0.005f, Color.yellow, duration: _traceDuration);
                DebugExt.DrawLine(_lastBottomPoint, p, Color.yellow, duration: _traceDuration);
                DebugExt.DrawRay(p, normal * 0.03f, Color.cyan, duration: _traceDuration);
                _lastBottomPoint = p;
            }
#endif

            var info = new CollisionInfo(_hitCount > 0, hitDistance, normal, hitPoint, surface);

            SetCollisionInfo(info, forceNotify);
            _lastUpdateFrame = frame;
        }

        private int DetectGround(Vector3 origin, float radius, float distance, RaycastHit[] hits) {
            var up = _transform.up;
            int hitCount = Physics.SphereCastNonAlloc(
                origin,
                radius,
                -up,
                hits,
                distance,
                _layerMask,
                QueryTriggerInteraction.Ignore
            );

#if UNITY_EDITOR
            if (_showDebugInfo) DebugExt.DrawSphereCast(origin, origin - up * distance, _radius, Color.yellow);
#endif

            hits.Filter(
                ref hitCount, 
                data: (self: this, origin, up), 
                predicate: (data, hit) => data.self.IsValidGroundHit(hit, data.origin, data.up)
            );

            return hitCount;
        }
        
        private bool TryDetectMotionNormal(Vector3 origin, Vector3 desiredMotion, out Vector3 normal) {
            var up = _transform.up;
            normal = up;
            var lowerCenter = origin + up * _distance;

            float minSqrDistance = 0f;
            int targetIndex = -1;
            
            // Trying to find contact point which is placed in front of current position
            // and has the closest distance to the ground cast origin, and use its normal.
            for (int i = 0; i < _hitCount; i++) {
                var hit = _raycastHitsMain[i];
                
                // Filter out contacts behind current position.
                if (Vector3.Dot(hit.point - lowerCenter, desiredMotion) < 0f) continue;
                
                float sqrDistance = Vector3.SqrMagnitude(origin - hit.point);
                if (targetIndex >= 0 && sqrDistance > minSqrDistance) continue;
                
                minSqrDistance = sqrDistance; 
                targetIndex = i;
            }

            if (targetIndex < 0) return false;
            
            normal = _raycastHitsMain[targetIndex].normal;
            return true;
        }
        
        private bool IsValidGroundHit(RaycastHit hit, Vector3 origin, Vector3 up) {
            return hit.distance > 0f &&
                   Vector3.ProjectOnPlane(hit.point - origin, up).sqrMagnitude < _radius * _radius;
        }

#if UNITY_EDITOR
        private Vector3 _lastBottomPoint;
        private void OnDrawGizmos() {
            if (!_showDebugInfo) return;

            var t = transform;
            var p = t.TransformPoint(Vector3.up * _bottomPoint);
            var r = t.right;
            
            DebugExt.DrawSphere(p, 0.02f, Color.yellow, gizmo: true);
            DebugExt.DrawLine(p + r * 0.2f, p - r * 0.2f, Color.yellow, gizmo: true);

            if (Application.isPlaying) {
                string text = CollisionInfo.hasContact ? "grounded" : "in air";
                DebugExt.DrawLabel(_originOffset + transform.TransformPoint(Vector3.up), text);
            }
        }
#endif
    }

}
