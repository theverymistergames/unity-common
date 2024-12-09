using System;
using MisterGames.Collisions.Core;
using MisterGames.Collisions.Utils;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Collisions.Detectors {

    public sealed class FrameRaycaster : CollisionDetectorBase, IUpdate {

        [SerializeField] private Transform _transform;
        [SerializeField] private PlayerLoopStage _timeSourceStage = PlayerLoopStage.Update;

        [Header("Raycast Settings")]
        [SerializeField] [Min(1)] private int _maxHits = 6;
        [SerializeField] [Min(0f)] private float _maxDistance = 3f;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore;

        public override Vector3 OriginOffset {
            get => _originOffset;
            set {
                if (_originOffset.IsNearlyEqual(value, tolerance: 0f)) return;

                _originOffset = value;
                _invalidateFlag = true;
            }
        }

        public override float Distance {
            get => _maxDistance;
            set {
                if (_maxDistance.IsNearlyEqual(value, tolerance: 0f)) return;

                _maxDistance = value;
                _invalidateFlag = true;
            }
        }

        public override int Capacity => _maxHits;

        private RaycastHit[] _raycastHits;
        private CollisionInfo[] _hits;

        private Vector3 _originOffset;
        private bool _invalidateFlag;

        private int _hitCount;
        private int _lastUpdateFrame = -1;

        private void Awake() {
            _raycastHits = new RaycastHit[_maxHits];
            _hits = new CollisionInfo[_maxHits];
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

        public void OnUpdate(float dt) {
            UpdateContacts();
        }

        public override void FetchResults() {
            UpdateContacts();
        }

        public override ReadOnlySpan<CollisionInfo> FilterLastResults(CollisionFilter filter) {
            int hitCount = _hitCount;
            
            _raycastHits
                .RemoveInvalidHits(ref hitCount)
                .Filter(ref hitCount, filter);

            if (hitCount <= 0) return ReadOnlySpan<CollisionInfo>.Empty;

            for (int i = 0; i < hitCount; i++) {
                _hits[i] = CollisionInfo.FromRaycastHit(_raycastHits[i]);
            }

            return ((ReadOnlySpan<CollisionInfo>) _hits)[..hitCount];
        }

        private void UpdateContacts(bool forceNotify = false) {
            if (!enabled) return;

            int frame = Time.frameCount;
            if (frame == _lastUpdateFrame && !_invalidateFlag) return;

            _invalidateFlag = false;

            bool hasContact = PerformRaycast(out var hit);
            var info = hasContact ? CollisionInfo.FromRaycastHit(hit) : CollisionInfo.Empty;

            SetCollisionInfo(info, forceNotify);
            _lastUpdateFrame = frame;
        }

        private bool PerformRaycast(out RaycastHit hit) {
            _hitCount = Physics.RaycastNonAlloc(
                _transform.position + _originOffset,
                _transform.forward,
                _raycastHits,
                _maxDistance,
                _layerMask,
                _triggerInteraction
            );

            return _raycastHits
                .RemoveInvalidHits(ref _hitCount)
                .TryGetMinimumDistanceHit(_hitCount, out hit);
        }
    }

}
