using System;
using MisterGames.Common.Layers;
using UnityEngine;

namespace MisterGames.Collisions.Rigidbodies {
    
    public sealed class CollisionEmitterFilter : CollisionEmitter {
        
        [SerializeField] private CollisionEmitter _collisionEmitter;
        [SerializeField] private LayerMask _layerMask;
        
        public override event CollisionCallback CollisionEnter = delegate { };
        public override event CollisionCallback CollisionExit = delegate { };
        
        private void OnEnable() {
            _collisionEmitter.CollisionEnter += HandleCollisionEnter;
            _collisionEmitter.CollisionExit += HandleCollisionExit;
        }

        private void OnDisable() {
            _collisionEmitter.CollisionEnter -= HandleCollisionEnter;
            _collisionEmitter.CollisionExit -= HandleCollisionExit;
        }

        private void HandleCollisionEnter(Collision collision) {
            if (!_layerMask.Contains(collision.collider.gameObject.layer)) return;
            
            CollisionEnter.Invoke(collision);
        }

        private void HandleCollisionExit(Collision collision) {
            if (!_layerMask.Contains(collision.collider.gameObject.layer)) return;
            
            CollisionExit.Invoke(collision);
        }
    }
    
}