using MisterGames.Collisions.Core;
using MisterGames.Collisions.Utils;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Collisions.Detectors {

    public class FrameSphereCaster : CollisionDetector, IUpdate {

        [SerializeField] private TimeDomain _timeDomain;

        [Header("Spherecast Settings")]
        [SerializeField] [Min(1)] private int _maxHits = 6;
        [SerializeField] [Min(0f)] private float _radius;
        [SerializeField] [Min(0f)] private float _maxDistance = 3f;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore;

        private Transform _transform;
        private RaycastHit[] _hits;
        private int _hitCount;
        private int _lastUpdateFrame = -1;

        private void Awake() {
            _transform = transform;
            _hits = new RaycastHit[_maxHits];
        }

        private void OnEnable() {
            _timeDomain.Source.Subscribe(this);
        }

        private void OnDisable() {
            _timeDomain.Source.Unsubscribe(this);
        }

        private void Start() {
            UpdateContacts(forceNotify: true);
        }

        void IUpdate.OnUpdate(float dt) {
            UpdateContacts();
        }

        public override void FetchResults() {
            UpdateContacts();
        }

        public override void FilterLastResults(CollisionFilter filter, out CollisionInfo info) {
            info = default;

            if (!CollisionInfo.hasContact) return;

            bool hasHit = _hits
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

        private void UpdateContacts(bool forceNotify = false) {
            int frame = Time.frameCount;
            if (frame == _lastUpdateFrame) return;

            bool hasContact = PerformRaycast(out var hit);

            var info = new CollisionInfo {
                hasContact = hasContact,
                lastDistance = CollisionInfo.lastDistance,
                lastNormal = CollisionInfo.lastNormal,
                lastHitPoint = CollisionInfo.lastHitPoint,
                transform = hit.transform
            };

            if (info.hasContact) {
                info.lastDistance = hit.distance;
                info.lastNormal = hit.normal;
                info.lastHitPoint = hit.point;
            }

            SetCollisionInfo(info, forceNotify);
            _lastUpdateFrame = frame;
        }

        private bool PerformRaycast(out RaycastHit hit) {
            _hitCount = Physics.SphereCastNonAlloc(
                _transform.position,
                _radius,
                _transform.forward,
                _hits,
                _maxDistance,
                _layerMask,
                _triggerInteraction
            );

            return _hits
                .RemoveInvalidHits(_hitCount, out _hitCount)
                .TryGetMinimumDistanceHit(_hitCount, out hit);
        }
    }

}
