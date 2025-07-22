using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    public sealed class TransformGravityOrientation : MonoBehaviour, IUpdate {

        [SerializeField] private Transform _transform;
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
            _transform.rotation = _transform.rotation.SlerpNonZero(targetRot, _rotationSmoothing, dt);
        }
        
#if UNITY_EDITOR
        private void Reset() {
            _transform = transform;
        }
#endif
    }
    
}