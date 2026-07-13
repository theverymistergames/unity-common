using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    public sealed class TransformGravityOrientation : MonoBehaviour, IUpdate {

        [SerializeField] private Transform _transform;
        [SerializeField] private Transform _rotateAroundPoint;
        [SerializeField] private GravityProvider _gravityProvider;
        [SerializeField] [Min(0f)] private float _rotationSmoothing = 10f;

        private Quaternion _initialRotation;

        private void Awake() {
            _initialRotation = transform.rotation;
        }

        private void OnEnable() {
            PlayerLoopStage.FixedUpdate.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            var targetRot = _initialRotation * Quaternion.FromToRotation(Vector3.down, _gravityProvider.GravityDirection);
            var currentRot = _transform.rotation;
            var newRot = currentRot.SlerpNonZero(targetRot, _rotationSmoothing, dt);

            if (_rotateAroundPoint != null) {
                var deltaRot = newRot * Quaternion.Inverse(currentRot);
                var pivot = _rotateAroundPoint.position;
                _transform.position = pivot + deltaRot * (_transform.position - pivot);
            }

            _transform.rotation = newRot;
        }
        
#if UNITY_EDITOR
        private void Reset() {
            _transform = transform;
        }
#endif
    }
    
}