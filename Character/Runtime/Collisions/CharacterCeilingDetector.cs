using System;
using MisterGames.Collisions.Core;
using MisterGames.Collisions.Utils;
using MisterGames.Common;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Collisions {

    public sealed class CharacterCeilingDetector : CollisionDetectorBase, IRadiusCollisionDetector, IUpdate {

        [SerializeField] private PlayerLoopStage _timeSourceStage = PlayerLoopStage.Update;
        
        [Header("Sphere cast settings")]
        [SerializeField] [Min(1)] private int _maxHits = 2;

        [SerializeField] private float _distance = 0.55f;
        [SerializeField] private float _distanceAddition = 0.15f;
        [SerializeField] private float _radius = 0.3f;

        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore;

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

        private Transform _transform;

        private RaycastHit[] _raycastHits;
        private CollisionInfo[] _hits;

        private int _hitCount;

        private Vector3 _originOffset;
        private int _lastUpdateFrame;
        private bool _invalidateFlag;

        private void Awake() {
            _transform = transform;
            _raycastHits = new RaycastHit[_maxHits];
            _hits = new CollisionInfo[_maxHits];
        }

        private void Start() {
            RequestCeiling(forceNotify: true);
        }

        private void OnEnable() {
            _timeSourceStage.Subscribe(this);
        }

        private void OnDisable() {
            _timeSourceStage.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            RequestCeiling();
        }

        public override void FetchResults() {
            RequestCeiling();
        }

        public override ReadOnlySpan<CollisionInfo> FilterLastResults(CollisionFilter filter) {
            int hitCount = _hitCount;
            
            _raycastHits
                .RemoveInvalidHits(ref hitCount)
                .Filter(ref hitCount, filter);

            if (hitCount <= 0) return ReadOnlySpan<CollisionInfo>.Empty;

            for (int i = 0; i < hitCount; i++) {
                _hits[i] = CollisionInfo.FromRaycastHit(_raycastHits[i]);
            }

            return ((ReadOnlySpan<CollisionInfo>) _hits)[..hitCount];
        }

        private void RequestCeiling(bool forceNotify = false) {
            if (!enabled) return;

            int frame = Time.frameCount;
            if (frame == _lastUpdateFrame && !_invalidateFlag) return;

            var origin = _originOffset + _transform.position;
            float distance = _distance + _distanceAddition;

            _hitCount = PerformSphereCast(origin, _radius, distance, _raycastHits);
            bool hasHits = _hitCount > 0;

            var up = _transform.up;
            
            Vector3 normal;
            Vector3 hitPoint;
            float hitDistance;
            Transform surface = null;

            if (hasHits) {
                var hit = _raycastHits[0];
                hitPoint = hit.point;
                normal = hit.normal;
                surface = hit.transform;
                hitDistance = hit.distance;
            }
            else {
                hitPoint = CollisionInfo.point;
                normal = -up;
                hitDistance = CollisionInfo.distance;
            }

            var info = new CollisionInfo(hasHits, hitDistance, normal, hitPoint, surface);
            
            SetCollisionInfo(info, forceNotify);
            _lastUpdateFrame = frame;
        }
        
        private int PerformSphereCast(Vector3 origin, float radius, float distance, RaycastHit[] hits) {
            int hitCount = Physics.SphereCastNonAlloc(
                origin,
                radius,
                _transform.up,
                hits,
                distance,
                _layerMask,
                _triggerInteraction
            );

            hits.RemoveInvalidHits(ref hitCount);

            return hitCount;
        }

        [Header("Debug")]
        [SerializeField] private bool _debugDrawHitPoint;
        [SerializeField] private bool _debugDrawCast;
        [SerializeField] private bool _debugDrawHasCeilingText;
        [SerializeField] private Vector3 _debugDrawHasCeilingTextOffset;

#if UNITY_EDITOR
        private void OnDrawGizmos() {
            if (!Application.isPlaying) return;
            
            var up = transform.up;
            
            if (_debugDrawHitPoint) {
                if (CollisionInfo.hasContact) {
                    DebugExt.DrawPointer(CollisionInfo.point, Color.yellow, 0.3f, gizmo: true);
                }
            }
            
            if (_debugDrawCast) {
                var start = transform.position;
                var end = start + up * _distance;
                DebugExt.DrawCapsule(start, end, _radius, Color.cyan, gizmo: true);
            }
            
            if (_debugDrawHasCeilingText) {
                string text = CollisionInfo.hasContact ? "has ceiling" : "no ceiling";
                DebugExt.DrawLabel(transform.position + _debugDrawHasCeilingTextOffset, text);
            }
        }
#endif
    }

}
