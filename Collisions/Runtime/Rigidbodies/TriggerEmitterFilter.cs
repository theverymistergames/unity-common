using MisterGames.Common.Layers;
using UnityEngine;

namespace MisterGames.Collisions.Rigidbodies {
    
    public sealed class TriggerEmitterFilter : TriggerEmitter {
        
        [SerializeField] private TriggerEmitter _triggerEmitter;
        [SerializeField] private LayerMask _layerMask;
        
        public override event TriggerCallback TriggerEnter = delegate { };
        public override event TriggerCallback TriggerExit = delegate { };
        public override event TriggerCallback TriggerStay = delegate { };
        
        private void OnEnable() {
            _triggerEmitter.TriggerEnter += HandleTriggerEnter;
            _triggerEmitter.TriggerStay += HandleTriggerStay;
            _triggerEmitter.TriggerExit += HandleTriggerExit;
        }

        private void OnDisable() {
            _triggerEmitter.TriggerEnter -= HandleTriggerEnter;
            _triggerEmitter.TriggerStay -= HandleTriggerStay;
            _triggerEmitter.TriggerExit -= HandleTriggerExit;
        }

        private void HandleTriggerEnter(Collider collider) {
            if (!enabled || !_layerMask.Contains(collider.gameObject.layer)) return;
            
            TriggerEnter.Invoke(collider);
        }

        private void HandleTriggerStay(Collider collider) {
            if (!enabled || !_layerMask.Contains(collider.gameObject.layer)) return;
            
            TriggerStay.Invoke(collider);
        }

        private void HandleTriggerExit(Collider collider) {
            if (!enabled || !_layerMask.Contains(collider.gameObject.layer)) return;
            
            TriggerExit.Invoke(collider);
        }
    }
    
}