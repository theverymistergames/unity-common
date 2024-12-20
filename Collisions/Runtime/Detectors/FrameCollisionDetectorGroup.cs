using System;
using MisterGames.Collisions.Core;
using MisterGames.Collisions.Utils;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Collisions.Detectors {

    public class FrameCollisionDetectorGroup : CollisionDetectorBase, IUpdate {

        [SerializeField] private PlayerLoopStage _timeSourceStage = PlayerLoopStage.Update;

        [Header("Filter")]
        [SerializeField] [Min(0f)] private float _maxDistance = 3f;
        [SerializeField] private LayerMask _layerMask;

        [SerializeField] private CollisionDetectorBase[] _detectors;

        public override Vector3 OriginOffset { get => Vector3.zero; set { } }
        public override float Distance { get => 0f; set { } }

        public override int Capacity {
            get {
                int sum = 0;
                for (int i = 0; i < _detectors.Length; i++) {
                    sum += _detectors[i].Capacity;
                }
                return sum;
            }
        }

        private CollisionInfo[] _hits;
        private CollisionInfo[] _obstacleHits;

        private void Awake() {
            _hits = new CollisionInfo[Capacity];
        }

        private void OnEnable() {
            _timeSourceStage.Subscribe(this);
        }

        private void OnDisable() {
            _timeSourceStage.Unsubscribe(this);
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

        void IUpdate.OnUpdate(float dt) {
            UpdateContacts();
        }

        private void UpdateContacts(bool forceNotify = false) {
            if (!enabled) return;

            PerformCast(out var info);
            SetCollisionInfo(info, forceNotify);
        }

        private void PerformCast(out CollisionInfo info) {
            var filter = new CollisionFilter { maxDistance = _maxDistance, layerMask = _layerMask };
            int hitCount = 0;
            float minDistance = -1f;
            info = CollisionInfo.Empty;

            for (int i = 0; i < _detectors.Length; i++) {
                var detector = _detectors[i];
                var hits = detector.FilterLastResults(filter);
                
                for (int h = 0; h < hits.Length; h++) {
                    _hits[hitCount++] = hits[h];
                }

                if (!hits.TryGetMinimumDistanceHit(hits.Length, out var nearestHit)) continue;

                if (minDistance < 0f || nearestHit.distance < minDistance) {
                    minDistance = nearestHit.distance;
                    info = nearestHit;
                }
            }
        }
    }

}
