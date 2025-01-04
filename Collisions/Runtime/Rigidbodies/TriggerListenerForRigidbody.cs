using System.Collections.Generic;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Collisions.Rigidbodies {
    
    public sealed class TriggerListenerForRigidbody : MonoBehaviour, IUpdate {

        [SerializeField] private TriggerEmitter _triggerEmitter;
        
        public delegate void TriggerCallback(Rigidbody rigidbody);
        
        public event TriggerCallback TriggerEnter = delegate { };
        public event TriggerCallback TriggerExit = delegate { };

        private readonly Dictionary<Collider, Rigidbody> _colliderToRigidbodyMap = new();
        private readonly Dictionary<Rigidbody, int> _rigidbodyColliderCountMap = new();
        private readonly List<Collider> _collidersBuffer = new();

        private void OnEnable() {
            _triggerEmitter.TriggerEnter += TriggerEntered;
            _triggerEmitter.TriggerExit += TriggerExited;
        }

        private void OnDisable() {
            _triggerEmitter.TriggerEnter -= TriggerEntered;
            _triggerEmitter.TriggerExit -= TriggerExited;
            
            foreach (var rb in _rigidbodyColliderCountMap.Keys) {
                TriggerExit.Invoke(rb);
            }
            
            _colliderToRigidbodyMap.Clear();
            _rigidbodyColliderCountMap.Clear();
            _collidersBuffer.Clear();
            
            PlayerLoopStage.Update.Unsubscribe(this);
        }

        private void TriggerEntered(Collider collider) {
            if (collider.attachedRigidbody is not {} rb || 
                !_colliderToRigidbodyMap.TryAdd(collider, rb)) 
            {
                return;
            }

            int oldCount = _rigidbodyColliderCountMap.GetValueOrDefault(rb);
            _rigidbodyColliderCountMap[rb] = oldCount + 1;

            if (oldCount <= 0) TriggerEnter.Invoke(rb);

            PlayerLoopStage.Update.Subscribe(this);
        }

        private void TriggerExited(Collider collider) {
            if (!_colliderToRigidbodyMap.Remove(collider, out var rb)) 
            {
                return;
            }
            
            int newCount = _rigidbodyColliderCountMap.GetValueOrDefault(rb) - 1;
            if (newCount > 0) {
                _rigidbodyColliderCountMap[rb] = newCount;
                return;
            }

            _rigidbodyColliderCountMap.Remove(rb);
            TriggerExit.Invoke(rb);
            
            if (_rigidbodyColliderCountMap.Count == 0) PlayerLoopStage.Update.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            _collidersBuffer.Clear();
            
            foreach (var (collider, _) in _colliderToRigidbodyMap) {
                if (collider == null || !collider.gameObject.activeSelf) _collidersBuffer.Add(collider);
            }

            for (int i = 0; i < _collidersBuffer.Count; i++) {
                TriggerExited(_collidersBuffer[i]);
            }
        }
    }
    
}