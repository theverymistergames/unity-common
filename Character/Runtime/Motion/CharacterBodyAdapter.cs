using MisterGames.Character.Collisions;
using MisterGames.Character.Core;
using MisterGames.Collisions.Core;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Motion {

    public class CharacterBodyAdapter : MonoBehaviour, ITransformAdapter {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] private CharacterController _characterController;

        public Vector3 Position {
            get => _body.position;
            set => _body.position = value;
        }

        public Quaternion Rotation {
            get => _body.rotation;
            set => _body.rotation = value;
        }

        private ICollisionDetector _groundDetector;
        private Transform _body;
        private float _stepOffset;

        private void Awake() {
            _groundDetector = _characterAccess.GetPipeline<ICharacterCollisionPipeline>().GroundDetector;

            _body = _characterController.transform;
            _stepOffset = _characterController.stepOffset;
        }

        public void Move(Vector3 delta) {
            if (_characterController.enabled) {
                _characterController.stepOffset = _groundDetector.CollisionInfo.hasContact.AsFloat() * _stepOffset;
                _characterController.Move(delta);
                return;
            }

            _body.Translate(delta, Space.World);
        }

        public void Rotate(Quaternion delta) {
            _body.rotation *= delta;
        }
    }

}
