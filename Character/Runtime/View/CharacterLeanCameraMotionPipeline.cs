using MisterGames.Character.Core;
using MisterGames.Common.GameObjects;
using MisterGames.Dbg.Draw;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.View {

    public sealed class CharacterLeanCameraMotionPipeline : CharacterPipelineBase, ICharacterLeanCameraMotionPipeline, IUpdate {

        [SerializeField] private CharacterAccess _characterAccess;

        [Header("Motion Settings")]
        [SerializeField] [Min(0f)] private float _cameraMotionWeight = 1f;
        [SerializeField] [Min(0f)] private float _smoothFactor = 1f;

        [Header("Amplitude")]
        [SerializeField] [Min(0f)] private float _baseAmplitude = 0.01f;
        [SerializeField] [Min(0f)] private float _forwardAmplitude = 0.01f;
        [SerializeField] [Min(0f)] private float _backwardAmplitude = 0.01f;
        [SerializeField] [Min(0f)] private float _sideAmplitude = 0.01f;
        [SerializeField] [Min(0f)] private float _rotationLever = 0.1f;
        [SerializeField] [Min(0f)] private float _rotationAmplitude = 0.1f;

        private ICharacterMotionPipeline _motion;
        private ITransformAdapter _head;
        private CameraContainer _cameraContainer;
        private CameraStateKey _cameraStateKey;

        private Vector3 _currentPositionOffset;
        private Vector3 _targetPositionOffset;

        private Quaternion _currentRotationOffset;
        private Quaternion _targetRotationOffset;

        private void Awake() {
            _motion = _characterAccess.GetPipeline<ICharacterMotionPipeline>();
            _head = _characterAccess.HeadAdapter;
            _cameraContainer = _characterAccess.GetPipeline<CharacterViewPipeline>().CameraContainer;
        }

        public override void SetEnabled(bool isEnabled) {
            if (isEnabled) {
                _cameraStateKey = _cameraContainer.CreateState(this, _cameraMotionWeight);
                TimeSources.Get(PlayerLoopStage.Update).Subscribe(this);
                return;
            }

            _cameraContainer.RemoveState(_cameraStateKey);
            TimeSources.Get(PlayerLoopStage.Update).Unsubscribe(this);
        }

        private void OnEnable() {
            SetEnabled(true);
        }

        private void OnDisable() {
            SetEnabled(false);
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

            _cameraContainer.SetPositionOffset(_cameraStateKey, _currentPositionOffset);
            _cameraContainer.SetRotationOffset(_cameraStateKey, _currentRotationOffset);

            DbgRay.Create().From(_head.Position).Dir(velocity).Color(Color.red).Draw();
            DbgRay.Create().From(_head.Position - lever).Dir(lever + _currentPositionOffset).Color(Color.green).Draw();
            DbgRay.Create().From(_head.Position - lever).Dir(_currentRotationOffset * lever).Color(Color.cyan).Draw();
        }
    }

}
