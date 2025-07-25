using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Collisions.Rigidbodies {
    
    public sealed class TriggerEmitterGroup : TriggerEmitter {
        
        [SerializeField] private TriggerEmitter[] _triggerEmitters;
        
        public override event TriggerCallback TriggerEnter = delegate { };
        public override event TriggerCallback TriggerExit = delegate { };
        public override event TriggerCallback TriggerStay = delegate { };

        public override IReadOnlyCollection<Collider> EnteredColliders => _enteredColliders;

        private readonly HashSet<Collider> _enteredColliders = new();
        
        private void OnEnable() {
            for (int i = 0; i < _triggerEmitters.Length; i++) {
                var emitter = _triggerEmitters[i];
             
                emitter.TriggerEnter += HandleTriggerEnter;
                emitter.TriggerStay += HandleTriggerStay;
                emitter.TriggerExit += HandleTriggerExit;
            }
        }

        private void OnDisable() {
            for (int i = 0; i < _triggerEmitters.Length; i++) {
                var emitter = _triggerEmitters[i];
             
                emitter.TriggerEnter -= HandleTriggerEnter;
                emitter.TriggerStay -= HandleTriggerStay;
                emitter.TriggerExit -= HandleTriggerExit;
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