using MisterGames.Collisions.Core;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Core2 {

    public class CharacterMotionAdapter : MonoBehaviour, ITransformAdapter {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] private CharacterController _characterController;

        public Vector3 Position {
            get => _motionTransform.position;
            set => _motionTransform.position = value;
        }

        public Quaternion Rotation {
            get => _motionTransform.rotation;
            set => _motionTransform.rotation = value;
        }

        private ICollisionDetector _groundDetector;
        private Transform _motionTransform;
        private float _stepOffset;

        private void Awake() {
            _groundDetector = _characterAccess.GroundDetector;
            _motionTransform = _characterController.transform;
            _stepOffset = _characterController.stepOffset;
        }

        public void Move(Vector3 delta) {
            if (_characterController.enabled) {
                _characterController.stepOffset = _groundDetector.CollisionInfo.hasContact.AsFloat() * _stepOffset;
                _characterController.Move(delta);
                return;
            }

            _motionTransform.Translate(delta, Space.World);
        }

        public void Rotate(Quaternion delta) {
            _motionTransform.rotation *= delta;
        }
    }

}
