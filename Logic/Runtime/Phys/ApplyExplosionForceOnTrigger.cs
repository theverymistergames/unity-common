using MisterGames.Collisions.Rigidbodies;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    public sealed class ApplyExplosionForceOnTrigger : MonoBehaviour {
        
        [Header("Trigger")]
        [SerializeField] private TriggerEmitter _triggerEmitter;
        [SerializeField] private bool _ignoreTriggers;
        
        [Header("Force")]
        [SerializeField] private float _force = 1f;
        [SerializeField] private float _forceUpMul = 1f;
        [SerializeField] private ForceMode _forceMode = ForceMode.VelocityChange;

        [Header("Radius")]
        [SerializeField] [Min(0f)] private float _radiusBase;
        [SerializeField] private bool _sumWithDistanceToCenter = true;

        private Transform _transform;
        
        private void Awake() {
            _transform = transform;
        }

        private void OnEnable() {
            _triggerEmitter.TriggerEnter += TriggerEnter;
        }

        private void OnDisable() {
            _triggerEmitter.TriggerEnter -= TriggerEnter;
        }

        private void TriggerEnter(Collider collider) {
            if (_ignoreTriggers && collider.isTrigger ||
                collider.attachedRigidbody is not { } rb || 
                rb.isKinematic) 
            {
                return;
            }

            var center = _transform.position;
            float radius = _radiusBase + _sumWithDistanceToCenter.AsFloat() * (rb.position - center).magnitude;
            
            rb.AddExplosionForce(_force, center, radius, _forceUpMul, _forceMode);
        }
    }
    
}