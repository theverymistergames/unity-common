using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Character.View;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Animations {

    public sealed class CharacterAnimationPipeline : CharacterPipelineBase, ICharacterAnimationPipeline {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] private CameraController _cameraController;
        [SerializeField] private PlayerLoopStage _playerLoopStage = PlayerLoopStage.Update;
        [SerializeField] [Range(0f, 0.5f)] private float _edgeInterpolationWeight;

        private ITransformAdapter _bodyAdapter;
        private ITimeSource _timeSource;
        private bool _isEnabled;

        private void Awake() {
            _bodyAdapter = _characterAccess.BodyAdapter;
            _timeSource = TimeSources.Get(_playerLoopStage);
        }

        private void OnEnable() {
            SetEnabled(true);
        }

        private void OnDisable() {
            SetEnabled(false);
        }

        public override void SetEnabled(bool isEnabled) {
            _isEnabled = isEnabled;
        }

        public async UniTask ApplyAnimation(
            object source,
            ICharacterAnimationPattern pattern,
            float duration,
            CancellationToken cancellationToken = default
        ) {
            if (!_isEnabled) return;

            if (duration <= 0f) {
                _bodyAdapter.Position += pattern.MapBodyPositionOffset(1f);
                _bodyAdapter.Rotation *= pattern.MapBodyRotationOffset(1f);
                return;
            }

            var startBodyPosition = _bodyAdapter.Position;
            var startBodyRotation = _bodyAdapter.Rotation;

            _cameraController.RegisterInteractor(source);
            float progress = 0f;

            while (_isEnabled && !cancellationToken.IsCancellationRequested) {
                float progressDelta = duration <= 0f ? 1f : _timeSource.DeltaTime / duration;
                progress = Mathf.Clamp01(progress + progressDelta);

                var currentBodyPosition = _bodyAdapter.Position;
                var currentBodyRotation = _bodyAdapter.Rotation;

                var mappedBodyPosition = startBodyPosition + pattern.MapBodyPositionOffset(progress);
                var mappedBodyRotation = startBodyRotation * pattern.MapBodyRotationOffset(progress);

                var mappedHeadPositionOffset = pattern.MapHeadPositionOffset(progress);
                var mappedHeadRotationOffset = pattern.MapHeadRotationOffset(progress);

                float edgeInterpolation = InterpolationUtils.GetEdgeInterpolation(_edgeInterpolationWeight, progress);

                var targetBodyPosition = mappedBodyPosition;
                var targetBodyRotation = mappedBodyRotation;

                // No need to interpolate body transform values while in the second half of animation
                if (progress < 0.5f) {
                    targetBodyPosition = Vector3.Lerp(currentBodyPosition, mappedBodyPosition, edgeInterpolation);
                    targetBodyRotation = Quaternion.Slerp(currentBodyRotation, mappedBodyRotation, edgeInterpolation);
                }

                var targetHeadPositionOffset = Vector3.Lerp(Vector3.zero, mappedHeadPositionOffset, edgeInterpolation);
                var targetHeadRotationOffset = Quaternion.Slerp(Quaternion.identity, mappedHeadRotationOffset, edgeInterpolation);

                _bodyAdapter.Position = targetBodyPosition;
                _bodyAdapter.Rotation = targetBodyRotation;

                _cameraController.SetPositionOffset(source, targetHeadPositionOffset);
                _cameraController.SetRotationOffset(source, targetHeadRotationOffset);

                if (progress >= 1f) break;

                await UniTask.Yield();
            }

            _cameraController.UnregisterInteractor(source);
        }
    }

}
