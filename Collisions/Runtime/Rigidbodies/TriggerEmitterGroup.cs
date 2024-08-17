using UnityEngine;

namespace MisterGames.Collisions.Rigidbodies {
    
    public sealed class TriggerEmitterGroup : TriggerEmitter {
        
        [SerializeField] private TriggerEmitter[] _triggerEmitters;
        
        public override event TriggerCallback TriggerEnter = delegate { };
        public override event TriggerCallback TriggerExit = delegate { };
        public override event TriggerCallback TriggerStay = delegate { };
        
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
            if (!enabled) return;
            
            TriggerEnter.Invoke(collider);
        }

        private void HandleTriggerStay(Collider collider) {
            if (!enabled) return;
            
            TriggerStay.Invoke(collider);
        }

        private void HandleTriggerExit(Collider collider) {
            if (!enabled) return;
            
            TriggerExit.Invoke(collider);
        }
    }
    
}