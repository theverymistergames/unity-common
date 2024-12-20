using System;
using MisterGames.Collisions.Core;
using MisterGames.Collisions.Utils;
using MisterGames.Common.Layers;
using MisterGames.Common.Maths;
using MisterGames.Input.Global;
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
        public override float Distance { get => _maxDistance; set => _maxDistance = value; }
        public override int Capacity => 1;

        private EventSystem _eventSystem;
        private CollisionInfo[] _hits;
        private Vector3 _originOffset;

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

        public override ReadOnlySpan<CollisionInfo> FilterLastResults(CollisionFilter filter) {
            int hitCount = CollisionInfo.hasContact.AsInt();
            _hits.Filter(ref hitCount, filter);
            
            return ((ReadOnlySpan<CollisionInfo>) _hits)[..hitCount];
        }

        private void UpdateContacts(bool forceNotify = false) {
            if (!enabled) return;

            bool hasContact = PerformRaycast(out var hit);
            var info = hasContact ? CollisionInfo.FromRaycastResult(hit) : CollisionInfo.Empty;

            _hits[0] = info;
            SetCollisionInfo(info, forceNotify);
        }

        private bool PerformRaycast(out RaycastResult hit) {
            if (_eventSystem.currentInputModule is not InputSystemUIInputModule inputModule) {
                hit = default;
                return false;
            }
            
            hit = inputModule.GetLastRaycastResult(GlobalInput.DeviceId);
            return hit.isValid && hit.distance <= _maxDistance && _layerMask.Contains(hit.gameObject.layer);
        }
    }

}
