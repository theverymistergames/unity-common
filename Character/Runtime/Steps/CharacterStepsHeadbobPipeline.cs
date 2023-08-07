using MisterGames.Character.Core;
using MisterGames.Character.Motion;
using MisterGames.Character.View;
using MisterGames.Common.Data;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Steps {

    public sealed class CharacterStepsHeadbobPipeline : CharacterPipelineBase, ICharacterStepsHeadbobPipeline, IUpdate {

        [SerializeField] private PlayerLoopStage _playerLoopStage = PlayerLoopStage.Update;
        [SerializeField] private CharacterAccess _characterAccess;

        [Header("Motion Settings")]
        [SerializeField] [Min(0f)] private float _cameraWeight = 1f;
        [SerializeField] [Min(0f)] private float _smooth = 1f;

        [Header("Headbob Settings")]
        [SerializeField] private float _minSpeed = 0.2f;
        [SerializeField] private float _baseAmplitude = 1f;
        [SerializeField] private float _baseAmplitudeRandom = 0.2f;

        [Header("Left Foot")]
        [SerializeField] private Vector3Parameter _positionOffsetLeft = Vector3Parameter.Default();
        [SerializeField] private Vector3Parameter _rotationOffsetLeft = Vector3Parameter.Default();

        [Header("Right Foot")]
        [SerializeField] private Vector3Parameter _positionOffsetRight = Vector3Parameter.Default();
        [SerializeField] private Vector3Parameter _rotationOffsetRight = Vector3Parameter.Default();

        private CharacterProcessorMass _mass;
        private CameraContainer _cameraContainer;
        private CameraStateKey _cameraStateKey;
        private ICharacterStepsPipeline _steps;
        private ITransformAdapter _head;

        private Vector3 _targetPositionOffset;
        private Vector3 _currentPositionOffset;

        private Quaternion _targetRotationOffset;
        private Quaternion _currentRotationOffset;

        private Vector3 _targetPositionAmplitude;
        private Vector3 _targetRotationAmplitude;

        private float _invertedSqrDistance;
        private float _stepProgress;
        private int _foot;

        private void Awake() {
            _mass = _characterAccess.GetPipeline<ICharacterMotionPipeline>().GetProcessor<CharacterProcessorMass>();
            _head = _characterAccess.HeadAdapter;
            _cameraContainer = _characterAccess.GetPipeline<ICharacterViewPipeline>().CameraContainer;
            _steps = _characterAccess.GetPipeline<ICharacterStepsPipeline>();
        }

        private void OnEnable() {
            SetEnabled(true);
        }

        private void OnDisable() {
            SetEnabled(false);
        }

        public override void SetEnabled(bool isEnabled) {
            if (isEnabled) {
                _cameraStateKey = _cameraContainer.CreateState(this, _cameraWeight);
                TimeSources.Get(_playerLoopStage).Subscribe(this);

                _steps.OnStep -= OnStep;
                _steps.OnStep += OnStep;
                return;
            }

            _cameraContainer.RemoveState(_cameraStateKey);
            TimeSources.Get(_playerLoopStage).Unsubscribe(this);

            _steps.OnStep -= OnStep;
        }

        private void OnStep(int foot, float distance, Vector3 point) {
            _foot = foot;
            _invertedSqrDistance = distance > 0f ? 1f / (distance * distance) : 0f;

            float baseAmplitude = _baseAmplitude + Random.Range(-_baseAmplitudeRandom, _baseAmplitudeRandom);
            var positionParameter = _foot == 0 ? _positionOffsetLeft : _positionOffsetRight;
            var rotationParameter = _foot == 0 ? _rotationOffsetLeft : _rotationOffsetRight;

            _targetPositionAmplitude = baseAmplitude * positionParameter.CreateMultiplier();
            _targetRotationAmplitude = baseAmplitude * rotationParameter.CreateMultiplier();

            _stepProgress = 0f;
        }

        public void OnUpdate(float dt) {
            var plainVelocity = Vector3.ProjectOnPlane(_mass.CurrentVelocity, _head.Rotation * Vector3.up);
            float sqrSpeed = plainVelocity.sqrMagnitude;

            if (sqrSpeed < _minSpeed * _minSpeed) {
                _stepProgress = 0f;
                _targetPositionOffset = Vector3.zero;
                _targetRotationOffset = Quaternion.identity;
            }
            else {
                _stepProgress = Mathf.Clamp01(_stepProgress + dt * sqrSpeed * _invertedSqrDistance);

                var positionParameter = _foot == 0 ? _positionOffsetLeft : _positionOffsetRight;
                var rotationParameter = _foot == 0 ? _rotationOffsetLeft : _rotationOffsetRight;

                _targetPositionOffset = positionParameter
                    .Evaluate(_stepProgress)
                    .Multiply(_targetPositionAmplitude);

                _targetRotationOffset = Quaternion.Euler(
                    rotationParameter
                        .Evaluate(_stepProgress)
                        .Multiply(_targetRotationAmplitude)
                );
            }

            _currentPositionOffset = Vector3.Lerp(_currentPositionOffset, _targetPositionOffset, dt * _smooth);
            _currentRotationOffset = Quaternion.Slerp(_currentRotationOffset, _targetRotationOffset, dt * _smooth);

            _cameraContainer.SetPositionOffset(_cameraStateKey, _currentPositionOffset);
            _cameraContainer.SetRotationOffset(_cameraStateKey, _currentRotationOffset);
        }
    }

}
