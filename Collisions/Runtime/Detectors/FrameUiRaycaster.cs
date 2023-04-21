using System;
using System.Collections.Generic;
using MisterGames.Collisions.Core;
using MisterGames.Collisions.Utils;
using MisterGames.Tick.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MisterGames.Collisions.Detectors {

    public class FrameUiRaycaster : CollisionDetectorBase, IUpdate {

        [SerializeField] private PlayerLoopStage _timeSourceStage = PlayerLoopStage.Update;
        [SerializeField] [Min(1)] private int _maxHits = 6;
        [SerializeField] private CollisionFilter _collisionFilter;

        public override int Capacity => _maxHits;

        private ITimeSource _timeSource => TimeSources.Get(_timeSourceStage);

        private readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();
        private CollisionInfo[] _hits;

        private EventSystem _eventSystem;
        private int _hitCount;
        private int _lastUpdateFrame = -1;

        private void Awake() {
            _hits = new CollisionInfo[_maxHits];
            _eventSystem = EventSystem.current;
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
            _raycastResults
                .RemoveInvalidHits(_hitCount, out int hitCount)
                .Filter(hitCount, filter, out int filterCount);

            if (filterCount <= 0) return ReadOnlySpan<CollisionInfo>.Empty;

            for (int i = 0; i < filterCount; i++) {
                _hits[i] = CollisionInfo.FromRaycastResult(_raycastResults[i]);
            }

            return ((ReadOnlySpan<CollisionInfo>) _hits)[..filterCount];
        }

        private void UpdateContacts(bool forceNotify = false) {
            int frame = TimeSources.FrameCount;
            if (frame == _lastUpdateFrame) return;

            var origin = new PointerEventData(_eventSystem) { position = Input.mousePosition };
            _eventSystem.RaycastAll(origin, _raycastResults);
            _hitCount = Math.Min(_raycastResults.Count, _maxHits);

            bool hasContact = _raycastResults
                .RemoveInvalidHits(_hitCount, out _hitCount)
                .Filter(_hitCount, _collisionFilter, out _hitCount)
                .TryGetMinimumDistanceHit(_hitCount, out var raycastResult);

            var info = hasContact ? CollisionInfo.FromRaycastResult(raycastResult) : CollisionInfo.Empty;

            SetCollisionInfo(info, forceNotify);
            _lastUpdateFrame = frame;
        }
    }

}
