﻿using System;
using MisterGames.Character.Access;
using MisterGames.Character.Collisions;
using MisterGames.Character.View;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Height {
    public class CharacterHeightPipeline : MonoBehaviour, ICharacterHeightPipeline, IUpdate {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] private PlayerLoopStage _playerLoopStage = PlayerLoopStage.Update;
        [SerializeField] [Range(0f, 1f)] private float _edgeInterpolationWeight = 0.1f;

        public event Action<float, float> OnHeightChanged = delegate {  };

        public float Height { get => _characterController.height; set => SetHeight(value); }
        public float TargetHeight => _targetHeight;

        public float Radius { get => _characterController.radius; set => SetRadius(value); }
        public float TargetRadius => _targetRadius;

        private CameraController _cameraController;
        private CharacterController _characterController;
        private ITransformAdapter _bodyAdapter;
        private CharacterGroundDetector _groundDetector;
        private CharacterCeilingDetector _ceilingDetector;
        private ITimeSource _timeSource;

        private float _initialHeight;
        private float _initialHeightCoeff;

        private float _sourceHeight;
        private float _targetHeight;

        private float _sourceRadius;
        private float _targetRadius;

        private float _heightChangeDuration;
        private float _heightChangeProgress;

        private ICharacterHeightChangePattern _heightChangePattern;
        private Action _onFinish;

        public void ApplyHeightChange(
            float targetHeight,
            float targetRadius,
            float duration,
            bool scaleDuration = true,
            ICharacterHeightChangePattern pattern = null,
            Action onFinish = null
        ) {
            _targetHeight = Mathf.Max(0f, targetHeight);
            _sourceHeight = _characterController.height;

            _targetRadius = Mathf.Max(0f, targetRadius);
            _sourceRadius = _characterController.radius;

            _onFinish = onFinish;

            _heightChangePattern = pattern ?? CharacterHeightChangePatternLinear.Instance;
            _heightChangeDuration = Mathf.Max(0f, duration);

            if (scaleDuration) _heightChangeDuration *= Mathf.Abs(_targetHeight - _sourceHeight) * _initialHeightCoeff;

            if (_heightChangeDuration <= 0f) {
                SetRadius(_targetRadius);
                SetHeight(_targetHeight);
                return;
            }

            if (_sourceHeight.IsNearlyEqual(_targetHeight, tolerance: 0f)) {
                SetRadius(_targetRadius);

                _heightChangeProgress = 1f;
                _onFinish?.Invoke();
                _onFinish = null;

                return;
            }

            // Start height change
            _heightChangeProgress = 0f;
            OnHeightChanged.Invoke(_heightChangeProgress, _heightChangeDuration);
        }

        private void SetHeight(float height) {
            height = Mathf.Max(0f, height);
            _sourceHeight = _characterController.height;

            if (_sourceHeight.IsNearlyEqual(_targetHeight, tolerance: 0f)) {
                _onFinish?.Invoke();
                _onFinish = null;
            }

            _heightChangePattern = CharacterHeightChangePatternLinear.Instance;
            _heightChangeDuration = 0f;

            _targetHeight = height;

            if (_sourceHeight.IsNearlyEqual(_targetHeight, tolerance: 0f)) {
                _heightChangeProgress = 1f;
                return;
            }

            // Start height change
            _heightChangeProgress = 0f;
            OnHeightChanged.Invoke(_heightChangeProgress, _heightChangeDuration);

            ApplyHeight(_targetHeight);
            ApplyHeadOffset(_targetHeight);

            // Finish height change
            _heightChangeProgress = 1f;
            OnHeightChanged.Invoke(_heightChangeProgress, _heightChangeDuration);

            _onFinish?.Invoke();
            _onFinish = null;
        }

        private void SetRadius(float radius) {
            radius = Mathf.Max(0f, radius);

            _sourceRadius = _characterController.radius;
            _targetRadius = radius;

            ApplyRadius(radius);
        }

        public void SetEnabled(bool isEnabled) {
            if (isEnabled) {
                _timeSource.Subscribe(this);
                _characterAccess.CameraController.RegisterInteractor(this);
                return;
            }

            _characterAccess.CameraController.UnregisterInteractor(this);
            _timeSource.Unsubscribe(this);
        }

        private void Awake() {
            _cameraController = _characterAccess.CameraController;
            _bodyAdapter = _characterAccess.BodyAdapter;
            _groundDetector = _characterAccess.GroundDetector;
            _ceilingDetector = _characterAccess.CeilingDetector;
            _characterController = _characterAccess.CharacterController;
            _timeSource = TimeSources.Get(_playerLoopStage);

            _initialHeight = _characterController.height;
            _initialHeightCoeff = _initialHeight <= 0f ? 1f : 1f / _initialHeight;

            _sourceHeight = _initialHeight;
            _targetHeight = _initialHeight;
            _heightChangeProgress = 1f;

            _sourceRadius = _characterController.radius;
            _targetRadius = _sourceRadius;
        }

        private void OnEnable() {
            SetEnabled(true);
        }

        private void OnDisable() {
            SetEnabled(false);
        }

        public void OnUpdate(float dt) {
            HeightChangeStep(dt);
        }

        private void HeightChangeStep(float dt) {
            float lastProgress = _heightChangeProgress;
            float progressDelta = _heightChangeDuration <= 0f ? 1f : dt / _heightChangeDuration;
            _heightChangeProgress = Mathf.Clamp01(_heightChangeProgress + progressDelta);

            if (lastProgress >= 1f && _heightChangeProgress >= 1f) return;

            float linearRadius = Mathf.Lerp(_sourceRadius, _targetRadius, _heightChangeProgress);
            ApplyRadius(linearRadius);

            float linearHeight = Mathf.Lerp(_sourceHeight, _targetHeight, _heightChangeProgress);
            float mappedHeight = _heightChangePattern.MapHeight(linearHeight);

            float edgeInterpolationFactor = CharacterHeightUtils.GetEdgeInterpolation(_edgeInterpolationWeight, _heightChangeProgress);
            float interpolatedHeight = Mathf.Lerp(linearHeight, mappedHeight, edgeInterpolationFactor);

            ApplyHeight(interpolatedHeight);
            ApplyHeadOffset(linearHeight);

            OnHeightChanged.Invoke(_heightChangeProgress, _heightChangeDuration);

            if (_heightChangeProgress >= 1f) {
                _onFinish?.Invoke();
                _onFinish = null;
            }
        }

        private void ApplyHeight(float height) {
            var center = 0.5f * (height - _initialHeight) * Vector3.up;
            float detectorDistance = height * 0.5f - _characterController.radius;
            float previousHeight = _characterController.height;

            _characterController.height = height;
            _characterController.center = center;

            _groundDetector.OriginOffset = center;
            _groundDetector.Distance = detectorDistance;
            _groundDetector.FetchResults();

            if (!_groundDetector.CollisionInfo.hasContact) {
                _bodyAdapter.Move(Vector3.up * (previousHeight - height));
            }
        }

        private void ApplyHeadOffset(float height) {
            var offset = (height - _initialHeight) * Vector3.up;

            var positionOffset = _heightChangePattern.MapHeadPositionOffset(height);
            var rotationOffset = _heightChangePattern.MapHeadRotationOffset(height);

            _cameraController.SetPositionOffset(this, offset + positionOffset);
            _cameraController.SetRotation(this, rotationOffset);
        }

        private void ApplyRadius(float radius) {
            _characterController.radius = radius;
            _groundDetector.Radius = radius;
            _ceilingDetector.Radius = radius;
        }
    }

}