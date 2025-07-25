using System;
using System.Collections.Generic;
using MisterGames.Collisions.Core;
using UnityEngine;

namespace MisterGames.Collisions.Rigidbodies {
    
    public sealed class TriggerListenerForRigidbody : MonoBehaviour {

        [SerializeField] private TriggerEmitter _triggerEmitter;
        
        public delegate void TriggerCallback(Rigidbody rigidbody);
        
        public event TriggerCallback TriggerEnter = delegate { };
        public event TriggerCallback TriggerExit = delegate { };

        public IReadOnlyCollection<Rigidbody> EnteredRigidbodies => _rigidbodyColliderCountMap.Keys;
        
        private readonly Dictionary<Collider, Rigidbody> _colliderToRigidbodyMap = new();
        private readonly Dictionary<Rigidbody, int> _rigidbodyColliderCountMap = new();

        public void Subscribe(RigidbodyTriggerEventType evt, TriggerCallback callback) {
            switch (evt) {
                case RigidbodyTriggerEventType.Enter:
                    TriggerEnter += callback;
                    break;
                
                case RigidbodyTriggerEventType.Exit:
                    TriggerExit += callback;
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(evt), evt, null);
            }
        }

        public void Unsubscribe(RigidbodyTriggerEventType eventType, TriggerCallback callback) {
            switch (eventType) {
                case RigidbodyTriggerEventType.Enter:
                    TriggerEnter -= callback;
                    break;
                
                case RigidbodyTriggerEventType.Exit:
                    TriggerExit -= callback;
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
        
        private void OnEnable() {
            _triggerEmitter.TriggerEnter += TriggerEntered;
            _triggerEmitter.TriggerExit += TriggerExited;
        }

        private void OnDisable() {
            _triggerEmitter.TriggerEnter -= TriggerEntered;
            _triggerEmitter.TriggerExit -= TriggerExited;
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
        }

        private void TriggerExited(Collider collider) {
            if (!_colliderToRigidbodyMap.Remove(collider, out var rb)) {
                return;
            }
            
            int newCount = _rigidbodyColliderCountMap.GetValueOrDefault(rb) - 1;
            if (newCount > 0) {
                _rigidbodyColliderCountMap[rb] = newCount;
                return;
            }

            _rigidbodyColliderCountMap.Remove(rb);
            TriggerExit.Invoke(rb);
        }
    }
    
}