using System;
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

        public Vector3 Position {
            get => _characterController.transform.position;
            set => _characterController.transform.position = value;
        }

        public Quaternion HeadRotation => _cameraController.Rotation;
        public Quaternion BodyRotation => _body.rotation;
        public Vector3 Velocity => _characterController.velocity;

        public Quaternion MotionInputRotation => _useBodyRotationForMotionInput ? BodyRotation : HeadRotation;
        private bool _useBodyRotationForMotionInput;

        private ITimeSource _timeSource => TimeSources.Get(_timeSourceStage);
        private float _stepOffset;
        private Func<Vector3,Vector3> _convert;

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

        public void EnableCharacterController(bool isEnabled) {
            _characterController.gameObject.SetActive(isEnabled);
        }

        public void EnableGravity(bool isEnabled) {
            _useBodyRotationForMotionInput = isEnabled;
            _massProcessor.EnableGravity(isEnabled);
        }

        public void SetMotionConverter(Func<Vector3, Vector3> convert) {
            _convert = convert;
        }

        public void TeleportTo(Vector3 position) {
            EnableCharacterController(false);
            Position = position;
            EnableCharacterController(true);
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

        private void Start() {
            EnableGravity(true);
        }

        void IUpdate.OnUpdate(float dt) {
            _characterController.stepOffset = _massProcessor.IsGrounded ? _stepOffset : 0f;

            var delta = _convert?.Invoke(_massProcessor.Velocity * dt) ?? _massProcessor.Velocity * dt;
            _characterController.Move(delta);
        }
    }

}
