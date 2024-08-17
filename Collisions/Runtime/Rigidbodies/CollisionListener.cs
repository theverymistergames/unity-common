using System;
using MisterGames.Common.Layers;
using UnityEngine;

namespace MisterGames.Collisions.Rigidbodies {
    
    public sealed class CollisionListener : CollisionEmitter {
        
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private bool _collideWithTriggers;
        
        public override event CollisionCallback CollisionEnter = delegate { };
        public override event CollisionCallback CollisionExit = delegate { };
        public override event CollisionCallback CollisionStay = delegate { };

        private void OnCollisionEnter(Collision collision) {
            if (!CanCollide(collision.collider)) return;
            
            CollisionEnter.Invoke(collision);
        }

        private void OnCollisionStay(Collision collision) {
            if (!CanCollide(collision.collider)) return;
            
            CollisionStay.Invoke(collision);
        }

        private void OnCollisionExit(Collision collision) {
            if (!CanCollide(collision.collider)) return;
            
            CollisionExit.Invoke(collision);
        }

        private bool CanCollide(Collider collider) {
            return enabled &&
                   _layerMask.Contains(collider.gameObject.layer) &&
                   (_collideWithTriggers || !collider.isTrigger);
        }
    }
    
}