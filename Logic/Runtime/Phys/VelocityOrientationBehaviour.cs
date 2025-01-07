using MisterGames.Common.Tick;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.Logic.Phys {
    
    [RequireComponent(typeof(Rigidbody))]
    public sealed class VelocityOrientationBehaviour : MonoBehaviour, IUpdate {
        
        [SerializeField] private Vector3 _rotationOffset;
        [SerializeField] [Min(0f)] private float _smoothing;
        [SerializeField] [Min(0f)] private float _minSpeed = 1f;
        [SerializeField] [Range(0f, 1f)] private float _randomRotation;

        private Transform _transform;
        private Rigidbody _rigidbody;
        
        private void Awake() {
            _transform = transform;
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void OnEnable() {
            var velocity = _rigidbody.linearVelocity;
            if (velocity.sqrMagnitude < _minSpeed * _minSpeed) return;

            _transform.rotation = GetOrientation(velocity);
            
            PlayerLoopStage.FixedUpdate.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            var velocity = _rigidbody.linearVelocity;
            if (velocity.sqrMagnitude < _minSpeed * _minSpeed) return;
            
            var targetRotation = GetOrientation(velocity);
            _transform.rotation = Quaternion.Slerp(_transform.rotation, targetRotation, dt * _smoothing);
        }

        private Quaternion GetOrientation(Vector3 velocity) {
            return Quaternion.LookRotation(velocity.normalized) * 
                   Quaternion.Euler(_rotationOffset) * 
                   Quaternion.Lerp(Quaternion.identity, Random.rotation, _randomRotation);
        }
    }
    
}