using System;
using MisterGames.Collisions.Core;
using MisterGames.Collisions.Utils;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Collisions.Detectors {

    public class FrameCollisionDetectorGroup : CollisionDetectorBase, IUpdate {

        [SerializeField] private PlayerLoopStage _timeSourceStage = PlayerLoopStage.Update;
        [SerializeField] private CollisionDetectorBase[] _detectors;
        [SerializeField] private CollisionFilter _collisionFilter;

        public override int Capacity {
            get {
                int sum = 0;
                for (int i = 0; i < _detectors.Length; i++) {
                    sum += _detectors[i].Capacity;
                }
                return sum;
            }
        }

        private ITimeSource _timeSource => TimeSources.Get(_timeSourceStage);

        private CollisionInfo[] _hits;
        private bool _lastHasContact;
        private float _lastDetectionDistance;
        private int _lastDetectionFrame = -1;

        private void Awake() {
            _hits = new CollisionInfo[Capacity];
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

        public override ReadOnlySpan<CollisionInfo> FilterLastResults(CollisionFilter filter) {
            int filterCount = 0;
            for (int d = 0; d < _detectors.Length; d++) {
                var hits = _detectors[d].FilterLastResults(filter);

                for (int h = 0; h < hits.Length; h++) {
                    _hits[filterCount++] = hits[h];
                }
            }

            return filterCount <= 0
                ? ReadOnlySpan<CollisionInfo>.Empty
                : ((ReadOnlySpan<CollisionInfo>) _hits)[..filterCount];
        }

        public override void FetchResults() {
            UpdateContacts();
        }

        public void OnUpdate(float dt) {
            UpdateContacts();
        }

        private void UpdateContacts(bool forceNotify = false) {
            int frame = TimeSources.FrameCount;
            for (int i = 0; i < _detectors.Length; i++) {
                var detector = _detectors[i];

                detector.FetchResults();

                var hits = detector.FilterLastResults(_collisionFilter);
                if (!hits.TryGetMinimumDistanceHit(hits.Length, out var hit)) continue;

                OnNewCollision(hit, frame, forceNotify);
            }
        }

        private void OnNewCollision(CollisionInfo info, int frame, bool forceNotify) {
            if (frame > _lastDetectionFrame ||
                !_lastHasContact && info.hasContact ||
                info.hasContact && info.distance < _lastDetectionDistance
            ) {
                _lastHasContact = info.hasContact;
                _lastDetectionDistance = info.distance;

                if (info.hasContact) _lastDetectionFrame = frame;

                SetCollisionInfo(info, forceNotify);
            }
        }
    }

}
