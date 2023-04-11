using MisterGames.Collisions.Core;
using MisterGames.Collisions.Utils;
using MisterGames.Common.Maths;
using MisterGames.Dbg.Draw;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Core2.Collisions {

    public class CharacterGroundDetector : CollisionDetectorBase, IUpdate {

        [SerializeField] private PlayerLoopStage _timeSourceStage = PlayerLoopStage.Update;

        [Header("Sphere cast settings")]
        [SerializeField] [Min(1)] private int _maxHits = 6;
        [SerializeField] private float _initialDistance = 0.5f;
        [SerializeField] private float _distanceAddition = 0.02f;
        [SerializeField] private float _radius = 0.5f;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore;

        [Header("Normal calculation")]
        [SerializeField] private float _hitPointElevation = 0.2f;
        [SerializeField] private float _normalSphereCastRadius = 0.05f;

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

        private ITimeSource _timeSource => TimeSources.Get(_timeSourceStage);
        private readonly Vector3 _groundDetectionDirection = Vector3.down;
        private Transform _transform;

        private RaycastHit[] _hitsMain;
        private RaycastHit[] _hitsNormal;
        private int _hitCount;

        private Vector3 _originOffset;
        private float _distance;

        private int _lastUpdateFrame = -1;
        private bool _invalidateFlag;

        private void Awake() {
            _transform = transform;
            _hitsMain = new RaycastHit[_maxHits];
            _hitsNormal = new RaycastHit[1];

            Distance = _initialDistance;
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

        void IUpdate.OnUpdate(float dt) {
            RequestGround();
        }

        public override void FetchResults() {
            RequestGround();
        }

        public override void FilterLastResults(CollisionFilter filter, out CollisionInfo info) {
            info = default;

            if (!CollisionInfo.hasContact) return;

            bool hasHit = _hitsMain
                .Filter(_hitCount, filter, out int filterCount)
                .TryGetMinimumDistanceHit(filterCount, out var hit);

            info = new CollisionInfo {
                hasContact = hasHit,
                lastDistance = hit.distance,
                lastNormal = hit.normal,
                lastHitPoint = hit.point,
                transform = hit.transform
            };
        }

        private void RequestGround(bool forceNotify = false) {
            int frame = Time.frameCount;
            if (frame == _lastUpdateFrame && !_invalidateFlag) return;

            var origin = OriginOffset + _transform.position;
            _hitCount = PerformSphereCast(origin, _radius, GetDistance(), _hitsMain);

            bool hasHits = _hitCount > 0;
            var normal = Vector3.zero;
            var hitPoint = Vector3.zero;
            float hitDistance;
            Transform surface = null;

            if (hasHits) {
                float minSqrMagnitude = -1f;

                for (int i = 0; i < _hitCount; i++) {
                    var hit = _hitsMain[i];

                    var point = hit.point;
                    hitPoint += point;

                    float sqrMagnitude = (origin - point).sqrMagnitude;
                    if (sqrMagnitude < minSqrMagnitude || minSqrMagnitude < 0f) {
                        minSqrMagnitude = sqrMagnitude;
                        surface = hit.transform;
                    }

                    if (ClarifyNormalAtPoint(point)) normal += _hitsNormal[0].normal;
                }

                normal = normal.normalized;
                hitPoint /= _hitCount;
                hitDistance = Vector3.Distance(origin, hitPoint);
            }
            else {
                normal = _groundDetectionDirection.Inverted().normalized;
                hitPoint = CollisionInfo.lastHitPoint;
                hitDistance = CollisionInfo.lastDistance;
            }

            var info = new CollisionInfo {
                hasContact = hasHits,
                lastDistance = hitDistance,
                lastNormal = normal,
                lastHitPoint = hitPoint,
                transform = surface
            };

            SetCollisionInfo(info, forceNotify);
            _lastUpdateFrame = frame;
        }

        private bool ClarifyNormalAtPoint(Vector3 point) {
            var origin = point - _groundDetectionDirection * _hitPointElevation;
            return PerformSphereCast(origin, _normalSphereCastRadius, _hitPointElevation, _hitsNormal) > 0;
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

        private float GetDistance() {
            return Distance + _distanceAddition;
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _debugDrawNormal;
        [SerializeField] private bool _debugDrawCast;
        [SerializeField] private bool _debugDrawHitPoint;
        [SerializeField] private bool _debugDrawIsGroundedText;
        [SerializeField] private Vector3 _debugDrawIsGroundedTextOffset;

        private void OnDrawGizmos() {
            if (!Application.isPlaying) return;
            
            if (_debugDrawNormal) {
                var start = CollisionInfo.hasContact ? CollisionInfo.lastHitPoint : OriginOffset + transform.position + _groundDetectionDirection * (GetDistance() + _radius);
                DbgRay.Create().From(start).Dir(CollisionInfo.lastNormal).Color(Color.blue).Arrow(0.1f).Draw();
            }

            if (_debugDrawHitPoint) {
                if (CollisionInfo.hasContact) {
                    DbgPointer.Create().Position(CollisionInfo.lastHitPoint).Size(0.3f).Color(Color.yellow).Draw();    
                }
            }
            
            if (_debugDrawCast) {
                var start = OriginOffset + transform.position;
                var end = start + _groundDetectionDirection * GetDistance();
                DbgCapsule.Create().From(start).To(end).Radius(_radius).Color(Color.cyan).Draw();
            }
            
            if (_debugDrawIsGroundedText) {
                string text = CollisionInfo.hasContact ? "grounded" : "in air";
                DbgText.Create().Text(text).Position(OriginOffset + transform.position + _debugDrawIsGroundedTextOffset).Draw();
            }
        }
#endif
    }

}
