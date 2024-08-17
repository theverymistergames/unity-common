using MisterGames.Common.Layers;
using UnityEngine;

namespace MisterGames.Collisions.Rigidbodies {
    
    public sealed class TriggerListener : TriggerEmitter {
        
        [SerializeField] private LayerMask _layerMask;
        
        public override event TriggerCallback TriggerEnter = delegate { };
        public override event TriggerCallback TriggerExit = delegate { };
        public override event TriggerCallback TriggerStay = delegate { };

        private void OnTriggerEnter(Collider collider) {
            if (!CanCollide(collider)) return;
            
            TriggerEnter.Invoke(collider);
        }

        private void OnTriggerStay(Collider collider) {
            if (!CanCollide(collider)) return;
            
            TriggerStay.Invoke(collider);
        }

        private void OnTriggerExit(Collider collider) {
            if (!CanCollide(collider)) return;
            
            TriggerExit.Invoke(collider);
        }

        private bool CanCollide(Collider collider) {
            return enabled && _layerMask.Contains(collider.gameObject.layer);
        }
    }
    
}