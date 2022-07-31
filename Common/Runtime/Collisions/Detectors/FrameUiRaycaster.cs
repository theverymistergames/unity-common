using System;
using System.Collections.Generic;
using System.Text;
using MisterGames.Common.Collisions.Core;
using MisterGames.Common.Collisions.Utils;
using MisterGames.Common.Routines;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MisterGames.Common.Collisions.Detectors {

    public class FrameUiRaycaster : CollisionDetector, IUpdate {

        [SerializeField] private TimeDomain _timeDomain;
        [SerializeField] private CollisionFilter _collisionFilter;

        private readonly List<RaycastResult> _hits = new List<RaycastResult>();
        private EventSystem _eventSystem;
        private int _hitCount;
        private int _lastUpdateFrame = -1;

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
                lastNormal = hit.worldNormal,
                lastHitPoint = hit.worldPosition,
                transform = hasHit ? hit.gameObject.transform : null
            };
        }

        private void UpdateContacts(bool forceNotify = false) {
            int frame = Time.frameCount;
            if (frame == _lastUpdateFrame) return;

            var origin = new PointerEventData(_eventSystem) { position = Input.mousePosition };
            _eventSystem.RaycastAll(origin, _hits);
            _hitCount = _hits.Count;

            bool hasContact = _hits
                .RemoveInvalidHits(_hitCount, out _hitCount)
                .Filter(_hitCount, _collisionFilter, out _hitCount)
                .TryGetMinimumDistanceHit(_hitCount, out var hit);

            var info = new CollisionInfo {
                hasContact = hasContact,
                lastDistance = CollisionInfo.lastDistance,
                lastNormal = CollisionInfo.lastNormal,
                lastHitPoint = CollisionInfo.lastHitPoint,
                transform = hasContact ? hit.gameObject.transform : null
            };

            if (info.hasContact) {
                info.lastDistance = hit.distance;
                info.lastNormal = hit.worldNormal;
                info.lastHitPoint = hit.worldPosition;
            }

            SetCollisionInfo(info, forceNotify);
            _lastUpdateFrame = frame;
        }
    }

}
