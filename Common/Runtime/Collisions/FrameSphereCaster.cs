using MisterGames.Common.Routines;
using UnityEngine;

namespace MisterGames.Common.Collisions {

    public class FrameSphereCaster : CollisionDetector, IUpdate {

        [SerializeField] private TimeDomain _timeDomain;

        [Header("SphereCast Settings")]
        [SerializeField] [Min(1)] private int _maxHits = 6;
        [SerializeField] private float _maxDistance = 3f;
        [SerializeField] private float _radius = 0.5f;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore;

        private Transform _transform;
        private RaycastHit[] _hits;

        private void Awake() {
            _transform = transform;
            _hits = new RaycastHit[_maxHits];
        }

        private void OnEnable() {
            _timeDomain.SubscribeUpdate(this);
        }

        private void OnDisable() {
            _timeDomain.UnsubscribeUpdate(this);
        }

        private void Start() {
            UpdateContacts(forceNotify: true);
        }

        void IUpdate.OnUpdate(float dt) {
            UpdateContacts();
        }

        private void UpdateContacts(bool forceNotify = false) {
            var info = new CollisionInfo {
                hasContact = PerformSphereCast(out var hit),
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
        }

        private bool PerformSphereCast(out RaycastHit hit) {
            hit = default;

            int hitCount = Physics.SphereCastNonAlloc(
                _transform.position,
                _radius,
                _transform.forward,
                _hits,
                _maxDistance,
                _layerMask,
                _triggerInteraction
            );

            if (hitCount <= 0) return false;

            float minDistance = -1f;
            int hitIndex = -1;

            for (int i = 0; i < hitCount; i++) {
                var nextHit = _hits[i];
                float distance = nextHit.distance;

                if (distance <= 0f) continue;

                if (distance < minDistance) {
                    hitIndex = i;
                    minDistance = distance;
                    continue;
                }

                hitIndex = i;
                minDistance = distance;
            }

            if (hitIndex < 0) return false;

            hit = _hits[hitIndex];
            return true;
        }
    }

}
