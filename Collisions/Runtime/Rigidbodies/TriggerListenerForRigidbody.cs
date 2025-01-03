using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Collisions.Rigidbodies {
    
    public sealed class TriggerListenerForRigidbody : MonoBehaviour {

        [SerializeField] private TriggerEmitter _triggerEmitter;
        
        public delegate void TriggerCallback(Rigidbody rigidbody);
        
        public event TriggerCallback TriggerEnter = delegate { };
        public event TriggerCallback TriggerExit = delegate { };

        private readonly Dictionary<Collider, Rigidbody> _colliderToRigidbodyMap = new();
        private readonly Dictionary<Rigidbody, int> _rigidbodyColliderCountMap = new();
        
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
        }

        private void TriggerEntered(Collider collider) {
            if (collider.attachedRigidbody is not {} rb || 
                !_colliderToRigidbodyMap.TryAdd(collider, rb)) 
            {
                return;
            }

            int count = _rigidbodyColliderCountMap.GetValueOrDefault(rb);
            _rigidbodyColliderCountMap[rb] = count + 1;

            if (count <= 0) TriggerEnter.Invoke(rb);
        }

        private void TriggerExited(Collider collider) {
            if (collider.attachedRigidbody is not {} rb || 
                !_colliderToRigidbodyMap.Remove(collider)) 
            {
                return;
            }
            
            int newCount = Mathf.Max(0, _rigidbodyColliderCountMap.GetValueOrDefault(rb) - 1);
            if (newCount > 0) {
                _rigidbodyColliderCountMap[rb] = newCount;
                return;
            }

            _rigidbodyColliderCountMap.Remove(rb);
            TriggerExit.Invoke(rb);
        }
    }
    
}