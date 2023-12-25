using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Collisions;
using MisterGames.Character.Core;
using MisterGames.Collisions.Core;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Pose {

    public sealed class CharacterCapsulePipeline : CharacterPipelineBase, ICharacterCapsulePipeline {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] private Transform _headRoot;
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private PlayerLoopStage _playerLoopStage = PlayerLoopStage.Update;

        public event ProgressCallback OnHeightChange = delegate { };
        public event ProgressCallback OnPoseChange = delegate { };

        public CharacterPoseType CurrentPose { get => _currentPose; set => SetPose(value); }
        public CharacterPoseType TargetPose { get; private set; }

        public float CurrentHeight { get => _characterController.height; set => SetHeight(value); }
        public float TargetHeight { get; private set; }

        public float CurrentRadius { get => _characterController.radius; set => SetRadius(value); }
        public float TargetRadius { get; private set; }

        public Vector3 ColliderTop => GetColliderTopPoint();
        public Vector3 ColliderCenter => GetColliderCenterPoint();
        public Vector3 ColliderBottom => GetColliderBottomPoint();

        public override bool IsEnabled { get => enabled; set => enabled = value; }

        private ICharacterPoseGraphPipeline _poseGraph;
        private ITransformAdapter _bodyAdapter;
        private IRadiusCollisionDetector _groundDetector;
        private IRadiusCollisionDetector _ceilingDetector;
        private ITimeSource _timeSource;
        private CancellationTokenSource _enableCts;

        private CharacterPoseType _currentPose;
        private Vector3 _headRootInitialPosition;
        private float _initialHeight;
        private byte _lastPoseChangeId;

        private void Awake() {
            _bodyAdapter = _characterAccess.BodyAdapter;
            var collisionPipeline = _characterAccess.GetPipeline<ICharacterCollisionPipeline>();
            _groundDetector = collisionPipeline.GroundDetector;
            _ceilingDetector = collisionPipeline.CeilingDetector;
            _timeSource = TimeSources.Get(_playerLoopStage);

            _headRootInitialPosition = _headRoot.localPosition;
            _initialHeight = _characterController.height;
            TargetHeight = _initialHeight;
            TargetRadius = _characterController.radius;
        }

        private void OnEnable() {
            _enableCts?.Cancel();
            _enableCts?.Dispose();
            _enableCts = new CancellationTokenSource();
        }

        private void OnDisable() {
            _enableCts?.Cancel();
            _enableCts?.Dispose();
            _enableCts = null;

            _headRoot.localPosition = _headRootInitialPosition;
        }

        public async UniTask ChangePose(
            CharacterPoseType targetPose,
            CharacterCapsuleSize capsuleSize,
            float duration,
            float changePoseAtProgress,
            CancellationToken cancellationToken = default
        ) {
            if (!enabled) return;

            byte changeId = ++_lastPoseChangeId;

            var sourcePose = _currentPose;
            float sourceHeight = _characterController.height;
            float sourceRadius = _characterController.radius;

            TargetPose = targetPose;
            TargetHeight = capsuleSize.colliderHeight;
            TargetRadius = capsuleSize.colliderRadius;

            if (duration <= 0f) {
                ApplyHeight(TargetHeight);
                ApplyRadius(TargetRadius);

                OnHeightChange.Invoke(progress: 1f, totalDuration: 0f);

                if (sourcePose != targetPose) {
                    _currentPose = targetPose;
                    OnPoseChange.Invoke(progress: 1f, totalDuration: 0f);
                }

                return;
            }

            float progress = 0f;
            if (changePoseAtProgress >= 1f) changePoseAtProgress = 1f;

            while (enabled && !cancellationToken.IsCancellationRequested && changeId == _lastPoseChangeId) {
                progress = Mathf.Clamp01(progress + _timeSource.DeltaTime / duration);

                float linearHeight = Mathf.Lerp(sourceHeight, TargetHeight, progress);
                float linearRadius = Mathf.Lerp(sourceRadius, TargetRadius, progress);

                ApplyHeight(linearHeight);
                ApplyRadius(linearRadius);

                OnHeightChange.Invoke(progress, duration);

                if (_currentPose != targetPose && progress >= changePoseAtProgress) {
                    _currentPose = targetPose;
                    OnPoseChange.Invoke(progress, duration);
                }

                if (progress >= 1f) break;

                await UniTask.Yield();
            }
        }

        private void SetPose(CharacterPoseType targetPose, bool forceNotify = false) {
            if (!enabled) return;

            ++_lastPoseChangeId;

            var sourcePose = _currentPose;
            TargetPose = targetPose;
            _currentPose = targetPose;

            if (forceNotify || sourcePose != targetPose) {
                OnPoseChange.Invoke(progress: 1f, totalDuration: 0f);
            }
        }

        private void SetHeight(float height, bool forceNotify = false) {
            if (!enabled) return;

            ++_lastPoseChangeId;

            float sourceHeight = _characterController.height;
            TargetHeight = height;

            ApplyHeight(TargetHeight);

            if (forceNotify || !height.IsNearlyEqual(sourceHeight)) {
                OnHeightChange.Invoke(progress: 1f, totalDuration: 0f);
            }
        }

        private void SetRadius(float radius) {
            if (!enabled) return;

            ++_lastPoseChangeId;

            TargetRadius = radius;
            ApplyRadius(TargetRadius);
        }

        private Vector3 GetColliderTopPoint() {
            return _bodyAdapter.Position + _characterController.center + _characterController.height * 0.5f * Vector3.up;
        }

        private Vector3 GetColliderCenterPoint() {
            return _bodyAdapter.Position + _characterController.center + _characterController.height * 0.5f * Vector3.up;
        }

        private Vector3 GetColliderBottomPoint() {
            return _bodyAdapter.Position + _characterController.center + _characterController.height * 0.5f * Vector3.down;
        }

        private void ApplyHeight(float height) {
            var center = (height - _initialHeight) * Vector3.up;
            var halfCenter = 0.5f * center;

            float detectorDistance = height * 0.5f - _characterController.radius;
            float previousHeight = _characterController.height;

            _headRoot.localPosition = center + _headRootInitialPosition;

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
