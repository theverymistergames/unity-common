using System.Collections.Generic;
using MisterGames.Common.Layers;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Collisions.Rigidbodies {

    public sealed class TriggerListener : TriggerEmitter, IUpdate {

        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private bool _collideWithTriggers = true;
        [SerializeField] private bool _clearCollidersOnDisable = true;

        public override event TriggerCallback TriggerEnter = delegate { };
        public override event TriggerCallback TriggerExit = delegate { };

        private readonly HashSet<Collider> _colliderSet = new();
        private readonly Dictionary<int, Collider> _hashToColliderMap = new();
        private readonly Dictionary<int, int> _colliderHashToStayFrameMap = new();
        private readonly List<int> _exitBuffer = new();
        private int _frameCount;
        private bool _enabled;

        private void OnEnable() {
            _enabled = true;
            
            if (_colliderSet.Count > 0) PlayerLoopStage.FixedUpdate.Subscribe(this);
        }

        private void OnDisable() {
            _enabled = false;
            
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);

            if (!_clearCollidersOnDisable) return;

            foreach (var c in _colliderSet) {
                TriggerExit.Invoke(c);
            }

            _colliderSet.Clear();
            _hashToColliderMap.Clear();
            _colliderHashToStayFrameMap.Clear();
        }
        
        public override IReadOnlyCollection<Collider> GetEnteredColliders() {
            _colliderSet.RemoveWhere(c => c == null || !c.enabled || c.gameObject is not { activeSelf: true, activeInHierarchy: true });
            return _colliderSet;
        }
        
        void IUpdate.OnUpdate(float dt) {
            int frameCount = _frameCount.ReturnThenIncrementUncheckedRef();

            foreach (var kvp in _colliderHashToStayFrameMap) {
                if (kvp.Value < frameCount) _exitBuffer.Add(kvp.Key);
            }

            for (int i = 0; i < _exitBuffer.Count; i++) {
                int hash = _exitBuffer[i];
                _colliderHashToStayFrameMap.Remove(hash);

                if (!_hashToColliderMap.Remove(hash, out var collider)) continue;

                _colliderSet.Remove(collider);
                TriggerExit.Invoke(collider);
            }

            _exitBuffer.Clear();

            if (_colliderSet.Count <= 0) PlayerLoopStage.FixedUpdate.Unsubscribe(this);
        }

        private void OnTriggerEnter(Collider collider) {
            if (!CanCollide(collider)) return;

            int hash = collider.GetHashCode();
            int count = _colliderHashToStayFrameMap.Count;

            _colliderHashToStayFrameMap[hash] = _frameCount;

            if (_colliderSet.Add(collider)) {
                _hashToColliderMap[hash] = collider;
                TriggerEnter.Invoke(collider);
            }
            
            if (count <= 0) PlayerLoopStage.FixedUpdate.Subscribe(this);
        }

        private void OnTriggerStay(Collider collider) {
            int hash = collider.GetHashCode();

            if (_colliderHashToStayFrameMap.ContainsKey(hash)) {
                _colliderHashToStayFrameMap[hash] = _frameCount;
                return;
            }

            OnTriggerEnter(collider);
        }

        private void OnTriggerExit(Collider collider) {
            int hash = collider.GetHashCode();
            if (!_colliderHashToStayFrameMap.Remove(hash)) return;

            _hashToColliderMap.Remove(hash);
            _colliderSet.Remove(collider);

            TriggerExit.Invoke(collider);
        }

        private bool CanCollide(Collider collider) {
            return _enabled &&
                   (_collideWithTriggers || !collider.isTrigger) &&
                   _layerMask.Contains(collider.gameObject.layer);
        }
    }

}
