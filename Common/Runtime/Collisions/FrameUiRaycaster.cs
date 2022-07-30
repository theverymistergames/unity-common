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
            var origin = new PointerEventData(_eventSystem) { position = Input.mousePosition };
            _eventSystem.RaycastAll(origin, _hits);

            bool hasContact = CollisionUtils.TryGetMinimumDistanceHit(_hits.Count, _hits, out var hit);

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
