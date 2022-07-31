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

        private void Start() {
            UpdateContacts(forceNotify: true);
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

        public override void FetchResults() {
            UpdateContacts();
        }

        void IUpdate.OnUpdate(float dt) {
            UpdateContacts();
        }

        private void UpdateContacts(bool forceNotify = false) {
            int frame = Time.frameCount;
            for (int i = 0; i < _detectorGroup.Length; i++) {
                var detector = _detectorGroup[i];

                detector.FetchResults();
                detector.FilterLastResults(_collisionFilter, out var info);
                OnNewCollision(info, frame, forceNotify);
            }
        }

        private void OnNewCollision(CollisionInfo info, int frame, bool forceNotify) {
            if (frame > _lastDetectionFrame ||
                !_lastHasContact && info.hasContact ||
                info.hasContact && info.lastDistance < _lastDetectionDistance)
            {
                SetLastCollision(info, frame, forceNotify);
            }
        }

        private void SetLastCollision(CollisionInfo info, int frame, bool forceNotify) {
            _lastHasContact = info.hasContact;
            _lastDetectionDistance = info.lastDistance;
            if (info.hasContact) _lastDetectionFrame = frame;

            SetCollisionInfo(info, forceNotify);
        }
    }

}
