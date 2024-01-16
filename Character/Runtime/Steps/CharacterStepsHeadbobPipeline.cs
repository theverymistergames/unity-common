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
        [SerializeField] [Min(0f)] private float _returnSmooth = 1f;

        [Header("Headbob Settings")]
        [SerializeField] private float _minSpeed = 0.2f;
        [SerializeField] private float _maxSpeed = 6f;
        [SerializeField] private float _baseAmplitude = 1f;
        [SerializeField] private float _baseAmplitudeRandom = 0.2f;
        [SerializeField] private AnimationCurve _baseAmplitudeBySpeed = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [SerializeField] private Vector3Parameter _positionOffset = Vector3Parameter.Default();
        [SerializeField] private Vector3Parameter _rotationOffset = Vector3Parameter.Default();

        public override bool IsEnabled { get => enabled; set => enabled = value; }

        private CharacterProcessorMass _mass;
        private CameraContainer _cameraContainer;
        private int _cameraStateId;
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
            _cameraStateId = _cameraContainer.CreateState(_cameraWeight);
            TimeSources.Get(_playerLoopStage).Subscribe(this);

            _steps.OnStep -= OnStep;
            _steps.OnStep += OnStep;

            _steps.OnStartMotionStep -= OnStep;
            _steps.OnStartMotionStep += OnStep;
        }

        private void OnDisable() {
            _cameraContainer.RemoveState(_cameraStateId);
            TimeSources.Get(_playerLoopStage).Unsubscribe(this);

            _steps.OnStep -= OnStep;
            _steps.OnStartMotionStep -= OnStep;
        }

        private void OnStep(int foot, float distance, Vector3 point) {
            _foot = foot;
            _invertedSqrDistance = distance > 0f ? 1f / (distance * distance) : 0f;

            float speedRatio = _maxSpeed > 0f ? _mass.CurrentVelocity.sqrMagnitude / (_maxSpeed * _maxSpeed) : 0f;
            float baseAmplitude = _baseAmplitude + Random.Range(-_baseAmplitudeRandom, _baseAmplitudeRandom);
            baseAmplitude *= _baseAmplitudeBySpeed.Evaluate(speedRatio);

            int footDir = _foot == 0 ? 1 : -1;

            _targetPositionAmplitude = baseAmplitude * _positionOffset.CreateMultiplier().Multiply(footDir, 1f, 1f);
            _targetRotationAmplitude = baseAmplitude * _rotationOffset.CreateMultiplier().Multiply(1f, footDir, footDir);

            _stepProgress = 0f;
        }

        public void OnUpdate(float dt) {
            var plainVelocity = Vector3.ProjectOnPlane(_mass.CurrentVelocity, _head.Rotation * Vector3.up);
            float sqrSpeed = plainVelocity.sqrMagnitude;
            float targetSmooth;

            if (sqrSpeed < _minSpeed * _minSpeed) {
                targetSmooth = _returnSmooth;
                _stepProgress = 0f;
                _targetPositionOffset = Vector3.zero;
                _targetRotationOffset = Quaternion.identity;
            }
            else {
                targetSmooth = _smooth;
                _stepProgress = Mathf.Clamp01(_stepProgress + dt * sqrSpeed * _invertedSqrDistance);
                _targetPositionOffset = _positionOffset.Evaluate(_stepProgress).Multiply(_targetPositionAmplitude);
                _targetRotationOffset = Quaternion.Euler(_rotationOffset.Evaluate(_stepProgress).Multiply(_targetRotationAmplitude));
            }

            _currentPositionOffset = Vector3.Lerp(_currentPositionOffset, _targetPositionOffset, dt * targetSmooth);
            _currentRotationOffset = Quaternion.Slerp(_currentRotationOffset, _targetRotationOffset, dt * targetSmooth);

            _cameraContainer.SetPositionOffset(_cameraStateId, _currentPositionOffset);
            _cameraContainer.SetRotationOffset(_cameraStateId, _currentRotationOffset);
        }
    }

}
