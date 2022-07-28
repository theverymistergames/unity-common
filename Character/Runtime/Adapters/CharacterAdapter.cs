using MisterGames.Character.Phys;
using MisterGames.Character.View;
using MisterGames.Common.Routines;
using UnityEngine;

namespace MisterGames.Character.Motion {

    public class CharacterAdapter : MonoBehaviour, IUpdate {

        [SerializeField] private TimeDomain _timeDomain;
        
        [SerializeField] private Transform _body;
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private MassProcessor _massProcessor;
        [SerializeField] private CameraController _cameraController;
        
        public Quaternion HeadRotation => _cameraController.Rotation;
        public Quaternion BodyRotation => _body.rotation;
        public Vector3 Velocity => _characterController.velocity;

        private float _stepOffset;

        public void RotateHead(float angle) {
            _cameraController.Rotate(this, Quaternion.Euler(angle, 0, 0));
        }
        
        public void RotateBody(float angle) {
            _body.rotation *= Quaternion.Euler(0, angle, 0);
        }

        public void Move(Vector3 direction) {
            _massProcessor.SetForce(this, direction);
        }

        public void ApplyImpulse(Vector3 impulse) {
            _massProcessor.ApplyImpulse(impulse);
        }

        public void TeleportTo(Vector3 targetPosition) {
            var currentPosition = _characterController.transform.position;
            var diff = targetPosition - currentPosition;

            _characterController.Move(diff);
            _massProcessor.ResetAllForces();
        }
        
        private void Awake() {
            _stepOffset = _characterController.stepOffset;
        }

        private void OnEnable() {
            _cameraController.RegisterInteractor(this);
            _massProcessor.RegisterForceSource(this);
            _timeDomain.SubscribeUpdate(this);
        }

        private void OnDisable() {
            _cameraController.UnregisterInteractor(this);
            _massProcessor.UnregisterForceSource(this);
            _timeDomain.UnsubscribeUpdate(this);
        }
        
        void IUpdate.OnUpdate(float dt) {
            _characterController.stepOffset = _massProcessor.IsGrounded ? _stepOffset : 0f; 
            _characterController.Move(_massProcessor.Velocity * dt);
        }
    }

}
