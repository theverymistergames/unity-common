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

        private EventSystem _eventSystem;
        private readonly List<RaycastResult> _hits = new List<RaycastResult>();
        private int _hitCount;

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

        private string HitsToText(int hitCount) {
            var sb = new StringBuilder();

            sb.AppendLine("Hits {");

            hitCount = Math.Min(hitCount, _hits.Count);
            for (int i = 0; i < hitCount; i++) {
                var hit = _hits[i];
                bool hasContact = hit.gameObject != null;

                sb.AppendLine($" - Hit[{i}] : {(hasContact ? $"{hit.gameObject.name}::{hit.distance}" : "none")}");
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        private void UpdateContacts(bool forceNotify = false) {
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
        }
    }

}
