using System;
using UnityEngine;

namespace MisterGames.Collisions.Rigidbodies {
    
    public sealed class CollisionEmitterGroup : CollisionEmitter {
        
        [SerializeField] private CollisionEmitter[] _collisionEmitters;
        
        public override event CollisionCallback CollisionEnter = delegate { };
        public override event CollisionCallback CollisionExit = delegate { };
        public override event CollisionCallback CollisionStay = delegate { };
        
        private void OnEnable() {
            for (int i = 0; i < _collisionEmitters.Length; i++) {
                var emitter = _collisionEmitters[i];
             
                emitter.CollisionEnter += HandleCollisionEnter;
                emitter.CollisionStay += HandleCollisionStay;
                emitter.CollisionExit += HandleCollisionExit;
            }
        }

        private void OnDisable() {
            for (int i = 0; i < _collisionEmitters.Length; i++) {
                var emitter = _collisionEmitters[i];
             
                emitter.CollisionEnter -= HandleCollisionEnter;
                emitter.CollisionStay -= HandleCollisionStay;
                emitter.CollisionExit -= HandleCollisionExit;
            }
        }

        private void HandleCollisionEnter(Collision collision) {
            if (!enabled) return;
            
            CollisionEnter.Invoke(collision);
        }

        private void HandleCollisionStay(Collision collision) {
            if (!enabled) return;
            
            CollisionStay.Invoke(collision);
        }

        private void HandleCollisionExit(Collision collision) {
            if (!enabled) return;
            
            CollisionExit.Invoke(collision);
        }
    }
    
}