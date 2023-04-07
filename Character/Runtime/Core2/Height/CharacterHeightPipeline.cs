using System;
using MisterGames.Character.Core2.Collisions;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Core2.Height {

    public class CharacterHeightPipeline : MonoBehaviour, ICharacterHeightPipeline, IUpdate {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] private PlayerLoopStage _playerLoopStage = PlayerLoopStage.Update;

        [Header("Height")]
        [SerializeField] private float _initialHeight = 1.8f;
        [SerializeField] private AnimationCurve _heightByRatio = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header("Height change up")]
        [SerializeField] private float _heightChangeUpSpeed = 1f;
        [SerializeField] private AnimationCurve _heightChangeUpSpeedMultiplierByHeight = AnimationCurve.Constant(0f, 1f, 1f);

        [Header("Height change down")]
        [SerializeField] private float _heightChangeDownSpeed = 1f;
        [SerializeField] private AnimationCurve _heightChangeDownSpeedMultiplierByHeight = AnimationCurve.Constant(0f, 1f, 1f);

        public event Action<float> OnHeightChange = delegate {  };

        public float CurrentHeight => _characterController.height;
        public float TargetHeight => _initialHeight * _targetRatio;

        private CharacterController _characterController;
        private CharacterGroundDetector _groundDetector;
        private ITimeSource _timeSource;

        private float _sourceRatio;
        private float _targetRatio;

        private float _speedMultiplier = 1f;
        private float _progress;

        public void SetEnabled(bool isEnabled) {
            if (isEnabled) {
                _timeSource.Subscribe(this);
                _characterAccess.CameraController.RegisterInteractor(this);
                return;
            }

            _characterAccess.CameraController.UnregisterInteractor(this);
            _timeSource.Unsubscribe(this);
        }

        public void SetHeight(float height) {
            float ratio = _initialHeight <= 0f
                ? height
                : height / _initialHeight;

            SetHeightRatio(ratio);
        }

        public void SetHeightRatio(float targetRatio) {
            _sourceRatio = _initialHeight <= 0f
                ? _characterController.height
                : _characterController.height / _initialHeight;

            _targetRatio = Mathf.Max(0f, targetRatio);

            if (_sourceRatio.IsNearlyEqual(_targetRatio, tolerance: 0f)) return;

            // Start height change
            _progress = 0f;
            OnHeightChange.Invoke(_progress);

            // Finish height change immediate
            _progress = 1f;
            ApplyHeightRatio(_targetRatio);
        }

        public void MoveToHeightRatio(float targetRatio, float speedMultiplier) {
            _speedMultiplier = Mathf.Max(0f, speedMultiplier);

            targetRatio = Mathf.Max(0f, targetRatio);
            if (targetRatio.IsNearlyEqual(_targetRatio, tolerance: 0f)) return;

            _sourceRatio = _initialHeight <= 0f
                ? _characterController.height
                : _characterController.height / _initialHeight;

            _targetRatio = Mathf.Max(0f, targetRatio);

            if (_sourceRatio.IsNearlyEqual(_targetRatio, tolerance: 0f)) return;

            // Start height change
            _progress = 0f;
            OnHeightChange.Invoke(_progress);
        }

        private void Awake() {
            _groundDetector = _characterAccess.GroundDetector;
            _characterController = _characterAccess.CharacterController;
            _timeSource = TimeSources.Get(_playerLoopStage);

            SetHeight(_initialHeight);
        }

        private void OnEnable() {
            SetEnabled(true);
        }

        private void OnDisable() {
            SetEnabled(false);
        }

        public void OnUpdate(float dt) {
            float height = _characterController.height;
            float delta = _speedMultiplier * dt;

            if (_sourceRatio < _targetRatio) {
                delta *= _heightChangeUpSpeed * _heightChangeUpSpeedMultiplierByHeight.Evaluate(height);
            }
            else if (_sourceRatio > _targetRatio) {
                delta *= _heightChangeDownSpeed * _heightChangeDownSpeedMultiplierByHeight.Evaluate(height);
            }
            else {
                delta = 1f;
            }

            _progress = Mathf.Clamp01(_progress + delta);
            ApplyHeightRatio(Mathf.Lerp(_sourceRatio, _targetRatio, _progress));
        }

        private void ApplyHeightRatio(float ratio) {
            if (_sourceRatio < _targetRatio) {
                if (ratio >= _targetRatio - Mathf.Epsilon) ratio = _targetRatio;
            }
            else if (_sourceRatio > _targetRatio) {
                if (ratio <= _targetRatio + Mathf.Epsilon) ratio = _targetRatio;
            }
            else {
                ratio = _targetRatio;
            }

            ApplyHeight(_initialHeight * _heightByRatio.Evaluate(ratio));
        }

        private void ApplyHeight(float height) {
            float previousHeight = _characterController.height;
            if (previousHeight.IsNearlyEqual(height, tolerance: 0f)) return;

            var diffToInitialHeightVector = (height - _initialHeight) * Vector3.up;

            _characterController.height = height;
            _characterController.center = diffToInitialHeightVector * 0.5f;
            _characterAccess.CameraController.SetPositionOffset(this, diffToInitialHeightVector);
            _groundDetector.Distance = height * 0.5f - _characterController.radius;

            if (!_groundDetector.CollisionInfo.hasContact) {
                _characterAccess.BodyAdapter.Move(Vector3.up * (previousHeight - height));
            }

            OnHeightChange.Invoke(_progress);
        }
    }

}
