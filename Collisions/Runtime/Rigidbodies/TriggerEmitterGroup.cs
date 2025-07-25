using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Collisions.Rigidbodies {
    
    public sealed class TriggerEmitterGroup : TriggerEmitter {
        
        [SerializeField] private TriggerEmitter[] _triggerEmitters;
        
        public override event TriggerCallback TriggerEnter = delegate { };
        public override event TriggerCallback TriggerExit = delegate { };
        public override event TriggerCallback TriggerStay = delegate { };

        public override IReadOnlyCollection<Collider> EnteredColliders => GetEnteredColliders();

        private readonly HashSet<Collider> _enteredColliders = new();
        
        private void OnEnable() {
            for (int i = 0; i < _triggerEmitters.Length; i++) {
                var emitter = _triggerEmitters[i];
             
                emitter.TriggerEnter += HandleTriggerEnter;
                emitter.TriggerStay += HandleTriggerStay;
                emitter.TriggerExit += HandleTriggerExit;
            }
            
            FilterColliders();
        }

        private void OnDisable() {
            for (int i = 0; i < _triggerEmitters.Length; i++) {
                var emitter = _triggerEmitters[i];
             
                emitter.TriggerEnter -= HandleTriggerEnter;
                emitter.TriggerStay -= HandleTriggerStay;
                emitter.TriggerExit -= HandleTriggerExit;
            }
        }
        
        private IReadOnlyCollection<Collider> GetEnteredColliders() {
            if (!enabled) FilterColliders();
            return _enteredColliders;
        }

        private void FilterColliders() {
            _enteredColliders.Clear();
            
            for (int i = 0; i < _triggerEmitters.Length; i++) {
                var emitter = _triggerEmitters[i];
                
                foreach (var collider in emitter.EnteredColliders) {
                    if (collider != null) _enteredColliders.Add(collider);
                }
            }
        }

        private void HandleTriggerEnter(Collider collider) {
            _enteredColliders.Add(collider);
            TriggerEnter.Invoke(collider);
        }

        private void HandleTriggerStay(Collider collider) {
            TriggerStay.Invoke(collider);
        }

        private void HandleTriggerExit(Collider collider) {
            _enteredColliders.Remove(collider);
            TriggerExit.Invoke(collider);
        }
    }
    
}