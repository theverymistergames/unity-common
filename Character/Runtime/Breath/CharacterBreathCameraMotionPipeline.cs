﻿using MisterGames.Character.Core;
using MisterGames.Character.View;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Breath {

    public sealed class CharacterBreathCameraMotionPipeline : CharacterPipelineBase, ICharacterBreathCameraMotionPipeline, IUpdate {

        [SerializeField] private CharacterAccess _characterAccess;

        [Header("Motion Settings")]
        [SerializeField] [Min(0f)] private float _cameraMotionWeight = 1f;
        [SerializeField] [Min(0f)] private float _smoothFactor = 1f;

        [Header("Amplitude")]
        [SerializeField] private float _positionAmplitude = 0.01f;
        [SerializeField] private float _positionAmplitudeRandom = 0.005f;
        [SerializeField] private float _rotationXYRatio = 0.2f;
        [SerializeField] private float _rotationAmplitude = 1f;
        [SerializeField] private float _rotationAmplitudeRandom = 0.3f;

        private ICharacterBreathPipeline _breath;
        private CameraContainer _cameraContainer;
        private CameraStateKey _cameraStateKey;

        private Vector3 _currentPositionOffset;
        private Vector3 _preTargetPositionOffset;
        private Vector3 _targetPositionOffset;

        private Quaternion _currentRotationOffset;
        private Quaternion _preTargetRotationOffset;
        private Quaternion _targetRotationOffset;

        private void Awake() {
            _breath = _characterAccess.GetPipeline<ICharacterBreathPipeline>();
            _cameraContainer = _characterAccess.GetPipeline<CharacterViewPipeline>().CameraContainer;
        }

        public override void SetEnabled(bool isEnabled) {
            if (isEnabled) {
                _breath.OnInhale -= OnInhale;
                _breath.OnInhale += OnInhale;

                _breath.OnExhale -= OnExhale;
                _breath.OnExhale += OnExhale;

                _cameraStateKey = _cameraContainer.CreateState(this, _cameraMotionWeight);
                TimeSources.Get(PlayerLoopStage.Update).Subscribe(this);
                return;
            }

            _breath.OnInhale -= OnInhale;
            _breath.OnExhale -= OnExhale;

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
            _preTargetPositionOffset = Vector3.Lerp(_preTargetPositionOffset, _targetPositionOffset, _smoothFactor * dt);
            _preTargetRotationOffset = Quaternion.SlerpUnclamped(_preTargetRotationOffset, _targetRotationOffset, _smoothFactor * dt);

            _currentPositionOffset = Vector3.Lerp(_currentPositionOffset, _preTargetPositionOffset, _smoothFactor * dt);
            _currentRotationOffset = Quaternion.SlerpUnclamped(_currentRotationOffset, _preTargetRotationOffset, _smoothFactor * dt);

            _cameraContainer.SetPositionOffset(_cameraStateKey, _currentPositionOffset);
            _cameraContainer.SetRotationOffset(_cameraStateKey, _currentRotationOffset);
        }

        private void OnInhale(float duration, float amplitude) {
            SetupNextTargetOffsets(1, amplitude);
        }

        private void OnExhale(float duration, float amplitude) {
            SetupNextTargetOffsets(-1, amplitude);
        }

        private void SetupNextTargetOffsets(int dir, float baseAmplitude) {
            float targetPositionAmplitude = (_positionAmplitude + Random.Range(-_positionAmplitudeRandom, _positionAmplitudeRandom)) *
                                            baseAmplitude;
            float targetRotationAmplitude = (_rotationAmplitude + Random.Range(-_rotationAmplitudeRandom, _rotationAmplitudeRandom)) *
                                            baseAmplitude;

            var positionOffset = targetPositionAmplitude * Random.onUnitSphere;
            _targetPositionOffset = positionOffset.WithY(dir * Mathf.Abs(positionOffset.y));

            _targetRotationOffset = Quaternion.Euler(
                dir * targetRotationAmplitude,
                Mathf.Sign(Random.Range(-1f, 1f)) * targetRotationAmplitude * _rotationXYRatio,
                0f
            );
        }
    }

}