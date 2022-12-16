using MisterGames.Character.Phys;
using MisterGames.Character.View;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Motion {

    public class CharacterAdapter : MonoBehaviour, IUpdate {

        [SerializeField] private PlayerLoopStage _timeSourceStage = PlayerLoopStage.Update;

        [SerializeField] private Transform _body;
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private MassProcessor _massProcessor;
        [SerializeField] private CameraController _cameraController;
        
        public Quaternion HeadRotation => _cameraController.Rotation;
        public Quaternion BodyRotation => _body.rotation;
        public Vector3 Velocity => _characterController.velocity;

        private ITimeSource _timeSource => TimeSources.Get(_timeSourceStage);
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

        public void SetPosition(Vector3 position) {
            _characterController.gameObject.SetActive(false);
            _characterController.transform.position = position;
            _characterController.gameObject.SetActive(true);
        }
        
        private void Awake() {
            _stepOffset = _characterController.stepOffset;
        }

        private void OnEnable() {
            _cameraController.RegisterInteractor(this);
            _massProcessor.RegisterForceSource(this);

            _timeSource.Subscribe(this);
        }

        private void OnDisable() {
            _timeSource.Unsubscribe(this);

            _cameraController.UnregisterInteractor(this);
            _massProcessor.UnregisterForceSource(this);
        }
        
        void IUpdate.OnUpdate(float dt) {
            _characterController.stepOffset = _massProcessor.IsGrounded ? _stepOffset : 0f; 
            _characterController.Move(_massProcessor.Velocity * dt);
        }
    }

}
