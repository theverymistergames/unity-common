using System;
using MisterGames.Collisions.Core;
using MisterGames.Collisions.Utils;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Collisions.Detectors {

    public class FrameSphereCaster : CollisionDetectorBase, IUpdate {

        [SerializeField] private PlayerLoopStage _timeSourceStage = PlayerLoopStage.Update;

        [Header("Spherecast Settings")]
        [SerializeField] [Min(1)] private int _maxHits = 6;
        [SerializeField] [Min(0f)] private float _radius;
        [SerializeField] [Min(0f)] private float _maxDistance = 3f;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore;

        public override int Capacity => _maxHits;

        private ITimeSource _timeSource => TimeSources.Get(_timeSourceStage);
        private Transform _transform;

        private RaycastHit[] _raycastHits;
        private CollisionInfo[] _hits;

        private int _hitCount;
        private int _lastUpdateFrame = -1;

        private void Awake() {
            _transform = transform;
            _raycastHits = new RaycastHit[_maxHits];
            _hits = new CollisionInfo[_maxHits];
        }

        private void OnEnable() {
            _timeSource.Subscribe(this);
        }

        private void OnDisable() {
            _timeSource.Unsubscribe(this);
        }

        private void Start() {
            UpdateContacts(forceNotify: true);
        }

        public void OnUpdate(float dt) {
            UpdateContacts();
        }

        public override void FetchResults() {
            UpdateContacts();
        }

        public override ReadOnlySpan<CollisionInfo> FilterLastResults(CollisionFilter filter) {
            _raycastHits
                .RemoveInvalidHits(_hitCount, out int hitCount)
                .Filter(hitCount, filter, out int filterCount);

            if (filterCount <= 0) return ReadOnlySpan<CollisionInfo>.Empty;

            for (int i = 0; i < filterCount; i++) {
                _hits[i] = CollisionInfo.FromRaycastHit(_raycastHits[i]);
            }

            return ((ReadOnlySpan<CollisionInfo>) _hits)[..filterCount];
        }

        private void UpdateContacts(bool forceNotify = false) {
            int frame = TimeSources.FrameCount;
            if (frame == _lastUpdateFrame) return;

            bool hasContact = PerformRaycast(out var hit);
            var info = hasContact ? CollisionInfo.FromRaycastHit(hit) : CollisionInfo.Empty;

            SetCollisionInfo(info, forceNotify);
            _lastUpdateFrame = frame;
        }

        private bool PerformRaycast(out RaycastHit hit) {
            _hitCount = Physics.SphereCastNonAlloc(
                _transform.position,
                _radius,
                _transform.forward,
                _raycastHits,
                _maxDistance,
                _layerMask,
                _triggerInteraction
            );

            return _raycastHits
                .RemoveInvalidHits(_hitCount, out _hitCount)
                .TryGetMinimumDistanceHit(_hitCount, out hit);
        }
    }

}
