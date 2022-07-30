using System.Collections.Generic;
using MisterGames.Common.Routines;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MisterGames.Common.Collisions {

    public class FrameUiRaycaster : CollisionDetector, IUpdate {

        [SerializeField] private TimeDomain _timeDomain;
        [SerializeField] private LayerMask _layerMask;

        private EventSystem _eventSystem;
        private readonly List<RaycastResult> _hits = new List<RaycastResult>();

        private void Awake() {
            _eventSystem = EventSystem.current;
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
                hasContact = PerformEventSystemRaycast(out var hit),
                lastDistance = CollisionInfo.lastDistance,
                lastNormal = CollisionInfo.lastNormal,
                lastHitPoint = CollisionInfo.lastHitPoint,
                transform = hit.gameObject.transform
            };

            if (info.hasContact) {
                info.lastDistance = hit.distance;
                info.lastNormal = hit.worldNormal;
                info.lastHitPoint = hit.worldPosition;
            }

            SetCollisionInfo(info, forceNotify);
        }

        private bool PerformEventSystemRaycast(out RaycastResult hit) {
            hit = default;

            var origin = new PointerEventData(_eventSystem) { position = Input.mousePosition };
            _eventSystem.RaycastAll(origin, _hits);

            int hitCount = _hits.Count;
            if (hitCount <= 0) return false;

            float minDistance = -1f;
            int hitIndex = -1;

            for (int i = 0; i < hitCount; i++) {
                var nextHit = _hits[i];
                float distance = nextHit.distance;

                if (distance <= 0f || !_layerMask.Contains(nextHit.gameObject.layer)) continue;

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
