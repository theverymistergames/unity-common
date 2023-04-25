using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Character.Collisions;
using MisterGames.Character.View;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Height {
    public class CharacterHeightPipeline : CharacterPipelineBase, ICharacterHeightPipeline {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] private CameraController _cameraController;
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private PlayerLoopStage _playerLoopStage = PlayerLoopStage.Update;

        public event Action<float, float> OnHeightChanged = delegate {  };

        public float Height { get => _characterController.height; set => SetHeight(value); }
        public float TargetHeight => _targetHeight;

        public float Radius { get => _characterController.radius; set => ApplyRadius(value); }
        public Vector3 CenterOffset => _characterController.center;

        private ITransformAdapter _bodyAdapter;
        private CharacterGroundDetector _groundDetector;
        private CharacterCeilingDetector _ceilingDetector;
        private ITimeSource _timeSource;

        private float _initialHeight;
        private float _targetHeight;

        private CancellationTokenSource _destroyCts;
        private CancellationTokenSource _enableCts;

        private byte _lastHeightChangeId;
        private bool _isEnabled;

        private void Awake() {
            _destroyCts = new CancellationTokenSource();

            _bodyAdapter = _characterAccess.BodyAdapter;

            var collisionPipeline = _characterAccess.GetPipeline<ICharacterCollisionPipeline>();
            _groundDetector = collisionPipeline.GroundDetector;
            _ceilingDetector = collisionPipeline.CeilingDetector;

            _timeSource = TimeSources.Get(_playerLoopStage);

            _initialHeight = _characterController.height;
            _targetHeight = _initialHeight;
        }

        private void OnDestroy() {
            _destroyCts.Cancel();
            _destroyCts.Dispose();
        }

        private void OnEnable() {
            SetEnabled(true);
        }

        private void OnDisable() {
            SetEnabled(false);
        }

        public override void SetEnabled(bool isEnabled) {
            _isEnabled = isEnabled;

            if (_isEnabled) {
                _cameraController.RegisterInteractor(this);
                return;
            }

            _cameraController.UnregisterInteractor(this);
        }

        public async UniTask ApplyHeightChange(
            float sourceHeight,
            float targetHeight,
            float duration,
            CancellationToken cancellationToken = default
        ) {
            if (!_isEnabled) return;

            byte id = ++_lastHeightChangeId;

            sourceHeight = Mathf.Max(0f, sourceHeight);
            _targetHeight = Mathf.Max(0f, targetHeight);

            if (duration <= 0f) {
                SetHeight(_targetHeight);
                return;
            }

            float progress = 0f;
            OnHeightChanged.Invoke(progress, duration);

            while (_isEnabled && !cancellationToken.IsCancellationRequested) {
                float progressDelta = _timeSource.DeltaTime / duration;
                progress = Mathf.Clamp01(progress + progressDelta);

                float linearHeight = Mathf.Lerp(sourceHeight, _targetHeight, progress);
                ApplyHeight(linearHeight);

                OnHeightChanged.Invoke(progress, duration);

                if (progress >= 1f || id != _lastHeightChangeId) break;

                await UniTask.Yield();
            }
        }

        private void SetHeight(float height) {
            if (!_isEnabled) return;

            float sourceHeight = _characterController.height;
            _targetHeight = Mathf.Max(0f, height);

            if (sourceHeight.IsNearlyEqual(_targetHeight, tolerance: 0f)) return;

            OnHeightChanged.Invoke(0f, 0f);
            ApplyHeight(_targetHeight);
            OnHeightChanged.Invoke(1f, 0f);
        }

        private void ApplyHeight(float height) {
            var center = (height - _initialHeight) * Vector3.up;
            var halfCenter = 0.5f * center;

            float detectorDistance = height * 0.5f - _characterController.radius;
            float previousHeight = _characterController.height;

            _cameraController.SetPositionOffset(this, center);

            _characterController.height = height;
            _characterController.center = halfCenter;

            _groundDetector.OriginOffset = halfCenter;
            _groundDetector.Distance = detectorDistance;
            _groundDetector.FetchResults();

            if (!_groundDetector.CollisionInfo.hasContact) {
                _bodyAdapter.Move(Vector3.up * (previousHeight - height));
            }
        }

        private void ApplyRadius(float radius) {
            _characterController.radius = radius;
            _groundDetector.Radius = radius;
            _ceilingDetector.Radius = radius;
        }
    }

}
