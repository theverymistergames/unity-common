using System;
using MisterGames.Collisions.Core;
using MisterGames.Collisions.Utils;
using MisterGames.Common.Layers;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace MisterGames.Collisions.Detectors {

    public sealed class FrameUiRaycaster : CollisionDetectorBase, IUpdate {

        [SerializeField] private PlayerLoopStage _timeSourceStage = PlayerLoopStage.Update;
        [SerializeField] [Min(0f)] private float _maxDistance = 3f;
        [SerializeField] private LayerMask _layerMask;

        public override Vector3 OriginOffset { get => Vector3.zero; set { } }

        public override float Distance {
            get => _maxDistance;
            set {
                if (_maxDistance.IsNearlyEqual(value, tolerance: 0f)) return;

                _maxDistance = value;
                _invalidateFlag = true;
            }
        }

        public override int Capacity => 1;

        private EventSystem _eventSystem;
        private CollisionInfo[] _hits;
        private Vector3 _originOffset;
        private bool _invalidateFlag;
        private int _lastUpdateFrame = -1;

        private void Awake() {
            _eventSystem = EventSystem.current;
            _hits = new CollisionInfo[1];
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
            int hitCount = CollisionInfo.hasContact.AsInt();
            _hits.Filter(hitCount, filter, out hitCount);
            
            return ((ReadOnlySpan<CollisionInfo>) _hits)[..hitCount];
        }

        private void UpdateContacts(bool forceNotify = false) {
            if (!enabled) return;

            int frame = Time.frameCount;
            if (frame == _lastUpdateFrame && !_invalidateFlag) return;

            _invalidateFlag = false;

            bool hasContact = PerformRaycast(out var hit);
            var info = hasContact ? CollisionInfo.FromRaycastResult(hit) : CollisionInfo.Empty;

            _hits[0] = info;
            
            SetCollisionInfo(info, forceNotify);
            _lastUpdateFrame = frame;
        }

        private bool PerformRaycast(out RaycastResult hit) {
            var origin = new PointerEventData(_eventSystem);
            var inputModule = (InputSystemUIInputModule) _eventSystem.currentInputModule;
            
            hit = inputModule.GetLastRaycastResult(origin.pointerId);
            return hit.distance <= _maxDistance && _layerMask.Contains(hit.gameObject.layer);
        }
    }

}
