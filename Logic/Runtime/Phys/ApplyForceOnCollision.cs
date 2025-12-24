using MisterGames.Collisions.Rigidbodies;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    public sealed class ApplyForceOnCollision : MonoBehaviour {
        
        [SerializeField] private CollisionEmitter _collisionEmitter;
        [SerializeField] private float _force;
        [SerializeField] private ForceMode _forceMode;
        
        private void OnEnable() {
            _collisionEmitter.CollisionEnter += CollisionEnter;
        }

        private void OnDisable() {
            _collisionEmitter.CollisionEnter -= CollisionEnter;
        }

        private void CollisionEnter(Collision collision) {
            if (collision.rigidbody is not {} rb || rb.isKinematic) return;

            var contact = collision.GetContact(0);
            rb.AddForceAtPosition(-collision.relativeVelocity * _force, contact.point, _forceMode);
        }
    }
    
}