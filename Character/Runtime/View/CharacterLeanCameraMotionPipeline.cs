using MisterGames.Character.Core;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.View {

    public sealed class CharacterLeanCameraMotionPipeline :
        CharacterPipelineBase,
        ICharacterLeanCameraMotionPipeline,
        IUpdate
    {
        [SerializeField] private PlayerLoopStage _playerLoopStage = PlayerLoopStage.Update;
        [SerializeField] private CharacterAccess _characterAccess;

        [Header("Motion Settings")]
        [SerializeField] [Min(0f)] private float _cameraMotionWeight = 1f;
        [SerializeField] [Min(0f)] private float _smoothFactor = 1f;

        [Header("Amplitude")]
        [SerializeField] private float _baseAmplitude = 0.01f;
        [SerializeField] private float _forwardAmplitude = 0.01f;
        [SerializeField] private float _backwardAmplitude = 0.01f;
        [SerializeField] private float _sideAmplitude = 0.01f;
        [SerializeField] private float _rotationLever = 0.1f;
        [SerializeField] private float _rotationAmplitude = 0.1f;

        public override bool IsEnabled { get => enabled; set => enabled = value; }

        private ICharacterMotionPipeline _motion;
        private CameraContainer _cameraContainer;
        private int _cameraStateId;

        private Vector3 _currentPositionOffset;
        private Vector3 _targetPositionOffset;

        private Quaternion _currentRotationOffset;
        private Quaternion _targetRotationOffset;

        private void Awake() {
            _motion = _characterAccess.GetPipeline<ICharacterMotionPipeline>();
            _cameraContainer = _characterAccess.GetPipeline<CharacterViewPipeline>().CameraContainer;
        }

        private void OnEnable() {
            _cameraStateId = _cameraContainer.CreateState(_cameraMotionWeight);
            TimeSources.Get(_playerLoopStage).Subscribe(this);
        }

        private void OnDisable() {
            _cameraContainer.RemoveState(_cameraStateId);
            TimeSources.Get(_playerLoopStage).Unsubscribe(this);
        }

        public void OnUpdate(float dt) {
            var input = _motion.MotionInput;
            var velocity = new Vector3(
                input.x * _baseAmplitude * _sideAmplitude,
                0f,
                input.y * _baseAmplitude * (input.y >= 0f ? _forwardAmplitude : _backwardAmplitude)
            );

            var lever = _rotationLever * Vector3.up;
            var positionOffsetToLever = (velocity + lever).normalized * _rotationLever;

            var targetPositionOffset = positionOffsetToLever - lever;
            var targetRotationOffset = Quaternion.FromToRotation(Vector3.up, positionOffsetToLever);
            targetRotationOffset = Quaternion.Slerp(Quaternion.identity, targetRotationOffset, _rotationAmplitude);

            _currentPositionOffset = Vector3.Lerp(_currentPositionOffset, targetPositionOffset, _smoothFactor * dt);
            _currentRotationOffset = Quaternion.Slerp(_currentRotationOffset, targetRotationOffset, _smoothFactor * dt);

            _cameraContainer.SetPositionOffset(_cameraStateId, _currentPositionOffset);
            _cameraContainer.SetRotationOffset(_cameraStateId, _currentRotationOffset);
        }
    }

}
