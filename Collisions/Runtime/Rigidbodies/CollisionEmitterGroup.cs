using UnityEngine;

namespace MisterGames.Collisions.Rigidbodies {
    
    public sealed class CollisionEmitterGroup : CollisionEmitter {
        
        [SerializeField] private CollisionEmitter[] _collisionEmitters;
        
        public override event CollisionCallback CollisionEnter = delegate { };
        public override event CollisionCallback CollisionExit = delegate { };
        
        private void OnEnable() {
            for (int i = 0; i < _collisionEmitters.Length; i++) {
                var emitter = _collisionEmitters[i];
             
                emitter.CollisionEnter += HandleCollisionEnter;
                emitter.CollisionExit += HandleCollisionExit;
            }
        }

        private void OnDisable() {
            for (int i = 0; i < _collisionEmitters.Length; i++) {
                var emitter = _collisionEmitters[i];
             
                emitter.CollisionEnter -= HandleCollisionEnter;
                emitter.CollisionExit -= HandleCollisionExit;
            }
        }

        private void HandleCollisionEnter(Collision collision) {
            CollisionEnter.Invoke(collision);
        }

        private void HandleCollisionExit(Collision collision) {
            CollisionExit.Invoke(collision);
        }
    }
    
}