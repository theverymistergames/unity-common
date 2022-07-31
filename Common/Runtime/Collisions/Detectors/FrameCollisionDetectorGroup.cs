using MisterGames.Common.Collisions.Core;
using MisterGames.Common.Routines;
using UnityEngine;

namespace MisterGames.Common.Collisions.Detectors {

    public class FrameCollisionDetectorGroup : CollisionDetector, IUpdate {

        [SerializeField] private TimeDomain _timeDomain;
        [SerializeField] private CollisionDetector[] _detectorGroup;
        [SerializeField] private CollisionFilter _collisionFilter;

        private bool _lastHasContact;
        private float _lastDetectionDistance;
        private int _lastDetectionFrame = -1;

        private void OnEnable() {
            _timeDomain.SubscribeUpdate(this);
        }

        private void OnDisable() {
            _timeDomain.UnsubscribeUpdate(this);
        }

        public override void FilterLastResults(CollisionFilter filter, out CollisionInfo info) {
            info = default;

            for (int i = 0; i < _detectorGroup.Length; i++) {
                var detector = _detectorGroup[i];

                detector.FilterLastResults(filter, out var currentInfo);

                if (currentInfo.hasContact &&
                    (!info.hasContact || currentInfo.lastDistance < info.lastDistance))
                {
                    info = currentInfo;
                }
            }
        }

        void IUpdate.OnUpdate(float dt) {
            int frame = Time.frameCount;

            for (int i = 0; i < _detectorGroup.Length; i++) {
                var detector = _detectorGroup[i];

                detector.FilterLastResults(_collisionFilter, out var info);
                OnNewCollision(info, frame);
            }
        }

        private void OnNewCollision(CollisionInfo info, int frame) {
            if (frame > _lastDetectionFrame ||
                !_lastHasContact && info.hasContact ||
                info.hasContact && info.lastDistance < _lastDetectionDistance)
            {
                SetLastCollision(info, frame);
            }
        }

        private void SetLastCollision(CollisionInfo info, int frame) {
            _lastHasContact = info.hasContact;
            _lastDetectionDistance = info.lastDistance;
            if (info.hasContact) _lastDetectionFrame = frame;

            SetCollisionInfo(info);
        }
    }

}
