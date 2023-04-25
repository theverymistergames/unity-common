using System;
using MisterGames.Collisions.Core;
using MisterGames.Collisions.Utils;
using MisterGames.Common.Maths;
using MisterGames.Dbg.Draw;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Collisions {

    public class CharacterGroundDetector : CollisionDetectorBase, IUpdate {

        [SerializeField] private PlayerLoopStage _timeSourceStage = PlayerLoopStage.Update;

        [Header("Sphere cast settings")]
        [SerializeField] [Min(1)] private int _maxHits = 6;
        [SerializeField] private float _distance = 0.55f;
        [SerializeField] private float _distanceAddition = 0.15f;
        [SerializeField] private float _radius = 0.3f;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore;

        [Header("Normal calculation")]
        [SerializeField] private float _hitPointElevation = 0.2f;
        [SerializeField] private float _normalSphereCastRadius = 0.05f;

        public override int Capacity => _maxHits;

        public Vector3 OriginOffset {
            get => _originOffset;
            set {
                if (_originOffset.IsNearlyEqual(value, tolerance: 0f)) return;

                _originOffset = value;
                _invalidateFlag = true;
            }
        }

        public float Distance {
            get => _distance;
            set {
                if (_distance.IsNearlyEqual(value, tolerance: 0f)) return;

                _distance = value;
                _invalidateFlag = true;
            }
        }

        public float Radius {
            get => _radius;
            set {
                if (_radius.IsNearlyEqual(value, tolerance: 0f)) return;

                _radius = value;
                _invalidateFlag = true;
            }
        }

        private ITimeSource _timeSource => TimeSources.Get(_timeSourceStage);
        private readonly Vector3 _groundDetectionDirection = Vector3.down;
        private Transform _transform;

        private RaycastHit[] _raycastHitsMain;
        private CollisionInfo[] _hitsMain;

        private RaycastHit[] _raycastHitsNormal;

        private int _hitCount;

        private Vector3 _originOffset;
        private int _lastUpdateFrame = -1;
        private bool _invalidateFlag;

        private void Awake() {
            _transform = transform;
            _raycastHitsMain = new RaycastHit[_maxHits];
            _raycastHitsNormal = new RaycastHit[1];
        }

        private void OnEnable() {
            _timeSource.Subscribe(this);
        }

        private void OnDisable() {
            _timeSource.Unsubscribe(this);
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
            _raycastHitsMain
                .RemoveInvalidHits(_hitCount, out int hitCount)
                .Filter(hitCount, filter, out int filterCount);

            if (filterCount <= 0) return ReadOnlySpan<CollisionInfo>.Empty;

            for (int i = 0; i < filterCount; i++) {
                _hitsMain[i] = CollisionInfo.FromRaycastHit(_raycastHitsMain[i]);
            }

            return ((ReadOnlySpan<CollisionInfo>) _hitsMain)[..filterCount];
        }

        private void RequestGround(bool forceNotify = false) {
            if (!enabled) return;

            int frame = Time.frameCount;
            if (frame == _lastUpdateFrame && !_invalidateFlag) return;

            var origin = _originOffset + _transform.position;
            float distance = _distance + _distanceAddition;
            _hitCount = PerformSphereCast(origin, _radius, distance, _raycastHitsMain);

            bool hasHits = _hitCount > 0;
            var normal = Vector3.zero;
            var hitPoint = Vector3.zero;
            float hitDistance;
            Transform surface = null;

            if (hasHits) {
                float minSqrMagnitude = -1f;

                for (int i = 0; i < _hitCount; i++) {
                    var hit = _raycastHitsMain[i];

                    var point = hit.point;
                    hitPoint += point;

                    float sqrMagnitude = (origin - point).sqrMagnitude;
                    if (sqrMagnitude < minSqrMagnitude || minSqrMagnitude < 0f) {
                        minSqrMagnitude = sqrMagnitude;
                        surface = hit.transform;
                    }

                    if (ClarifyNormalAtPoint(point)) normal += _raycastHitsNormal[0].normal;
                }

                normal = normal.normalized;
                hitPoint /= _hitCount;
                hitDistance = Vector3.Distance(origin, hitPoint);
            }
            else {
                normal = _groundDetectionDirection.Inverted().normalized;
                hitPoint = CollisionInfo.point;
                hitDistance = CollisionInfo.distance;
            }

            var info = new CollisionInfo(hasHits, hitDistance, normal, hitPoint, surface);

            SetCollisionInfo(info, forceNotify);
            _lastUpdateFrame = frame;
        }

        private bool ClarifyNormalAtPoint(Vector3 point) {
            var origin = point - _groundDetectionDirection * _hitPointElevation;
            return PerformSphereCast(origin, _normalSphereCastRadius, _hitPointElevation, _raycastHitsNormal) > 0;
        }

        private int PerformSphereCast(Vector3 origin, float radius, float distance, RaycastHit[] hits) {
            return Physics.SphereCastNonAlloc(
                origin,
                radius,
                _groundDetectionDirection,
                hits,
                distance,
                _layerMask,
                _triggerInteraction
            );
        }

        [Header("Debug")]
        [SerializeField] private bool _debugDrawNormal;
        [SerializeField] private bool _debugDrawCast;
        [SerializeField] private bool _debugDrawHitPoint;
        [SerializeField] private bool _debugDrawIsGroundedText;
        [SerializeField] private Vector3 _debugDrawIsGroundedTextOffset;

#if UNITY_EDITOR
        private void OnDrawGizmos() {
            if (!Application.isPlaying) return;
            
            if (_debugDrawNormal) {
                var start = CollisionInfo.hasContact ?
                    CollisionInfo.point :
                    _originOffset + transform.position + _groundDetectionDirection * (_distance + _distanceAddition + _radius);

                DbgRay.Create().From(start).Dir(CollisionInfo.normal).Color(Color.blue).Arrow(0.1f).Draw();
            }

            if (_debugDrawHitPoint) {
                if (CollisionInfo.hasContact) {
                    DbgPointer.Create().Position(CollisionInfo.point).Size(0.3f).Color(Color.yellow).Draw();
                }
            }
            
            if (_debugDrawCast) {
                var start = _originOffset + transform.position;
                var end = start + _groundDetectionDirection * (_distance + _distanceAddition);
                DbgCapsule.Create().From(start).To(end).Radius(_radius).Color(Color.cyan).Draw();
            }
            
            if (_debugDrawIsGroundedText) {
                string text = CollisionInfo.hasContact ? "grounded" : "in air";
                DbgText.Create().Text(text).Position(_originOffset + transform.position + _debugDrawIsGroundedTextOffset).Draw();
            }
        }
#endif
    }

}
