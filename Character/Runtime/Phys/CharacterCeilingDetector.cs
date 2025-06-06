using System;
using MisterGames.Collisions.Core;
using MisterGames.Collisions.Utils;
using MisterGames.Common;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Character.Phys {

    public sealed class CharacterCeilingDetector : CollisionDetectorBase, IRadiusCollisionDetector, IUpdate {
        
        [Header("Sphere cast settings")]
        [SerializeField] [Min(1)] private int _maxHits = 2;

        [SerializeField] private float _distance = 0.55f;
        [SerializeField] private float _distanceAddition = 0.15f;
        [SerializeField] private float _radius = 0.3f;
        [SerializeField] private Vector3 _originOffset;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore;

        public override int Capacity => _maxHits;
        public override Vector3 OriginOffset { get => _originOffset; set => _originOffset = value; }
        public override float Distance { get => _distance; set => _distance = value; }
        public float Radius { get => _radius; set => _radius = value; }

        private Transform _transform;
        private RaycastHit[] _raycastHits;
        private CollisionInfo[] _hits;
        private int _hitCount;

        private void Awake() {
            _transform = transform;
            _raycastHits = new RaycastHit[_maxHits];
            _hits = new CollisionInfo[_maxHits];
        }

        private void OnEnable() {
            PlayerLoopStage.Update.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.Update.Unsubscribe(this);
        }

        private void Start() {
            RequestCeiling(forceNotify: true);
        }

        void IUpdate.OnUpdate(float dt) {
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

            var origin = _transform.TransformPoint(_originOffset);
            float distance = _distance + _distanceAddition;

            _hitCount = PerformSphereCast(origin, _radius, distance, _raycastHits);
            bool hasHits = _hitCount > 0;

            var up = _transform.up;
            
            Vector3 normal;
            Vector3 hitPoint;
            float hitDistance;
            Transform surface = null;
            Rigidbody rigidbody = null;
            Collider collider = null;
            bool isValid = false;
            
            if (hasHits) {
                var hit = _raycastHits[0];
                hitPoint = hit.point;
                normal = hit.normal;
                surface = hit.transform;
                hitDistance = hit.distance;
                rigidbody = hit.rigidbody;
                collider = hit.collider;
                isValid = hit.colliderInstanceID != 0;
            }
            else {
                hitPoint = CollisionInfo.point;
                normal = -up;
                hitDistance = CollisionInfo.distance;
            }

            var info = new CollisionInfo(hasHits, hitDistance, normal, hitPoint, surface, rigidbody, collider, isValid);
            
            SetCollisionInfo(info, forceNotify);
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
                var start = transform.TransformPoint(_originOffset);
                var end = start + up * (_distance + _distanceAddition);
                DebugExt.DrawSphereCast(start, end, _radius, Color.cyan, gizmo: true);
            }
            
            if (_debugDrawHasCeilingText) {
                string text = CollisionInfo.hasContact ? "has ceiling" : "no ceiling";
                DebugExt.DrawLabel(transform.position + _debugDrawHasCeilingTextOffset, text);
            }
        }
#endif
    }

}
