﻿using MisterGames.Common.Routines;
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
            int hitCount = Physics.SphereCastNonAlloc(
                _transform.position,
                _radius,
                _transform.forward,
                _hits,
                _maxDistance,
                _layerMask,
                _triggerInteraction
            );

            bool hasContact = CollisionUtils.TryGetMinimumDistanceHit(hitCount, _hits, out var hit);

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
        }
    }

}
