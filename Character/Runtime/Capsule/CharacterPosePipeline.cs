using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Conditions;
using MisterGames.Character.Core;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Capsule {

    public sealed class CharacterPosePipeline : CharacterPipelineBase, ICharacterPosePipeline {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] private PlayerLoopStage _playerLoopStage;

        public event OnPoseChanged OnPoseChanged = delegate {  };

        public CharacterPoseType CurrentPose { get => _currentPose; set => SetPose(value); }
        public CharacterPoseType TargetPose { get; private set; }

        public CharacterCapsuleSize CurrentCapsuleSize { get => _capsule.CapsuleSize; set => _capsule.CapsuleSize = value; }
        public CharacterCapsuleSize TargetCapsuleSize { get; private set; }

        public override bool IsEnabled { get => enabled; set => enabled = value; }

        private ICharacterCapsulePipeline _capsule;
        private ITimeSource _timeSource;

        private CharacterPoseType _currentPose;
        private byte _lastPoseChangeId;

        private void Awake() {
            _timeSource = TimeSources.Get(_playerLoopStage);
            _capsule = _characterAccess.GetPipeline<ICharacterCapsulePipeline>();
        }

        public async UniTask<bool> TryChangePose(
            CharacterPoseType targetPose,
            CharacterCapsuleSize capsuleSize,
            float duration,
            float setTargetPoseAt = 0f,
            ICharacterCondition canContinue = null,
            CancellationToken cancellationToken = default
        ) {
            if (!enabled) return false;

            byte changeId = ++_lastPoseChangeId;
            bool checkCondition = canContinue != null;

            var sourcePose = _currentPose;
            TargetPose = targetPose;
            TargetCapsuleSize = capsuleSize;

            if (checkCondition && !canContinue.IsMatch(_characterAccess)) return false;

            if (duration <= 0f) {
                _capsule.CapsuleSize = capsuleSize;

                if (sourcePose != targetPose) {
                    _currentPose = targetPose;
                    OnPoseChanged.Invoke(targetPose, sourcePose);
                }

                return !checkCondition || canContinue.IsMatch(_characterAccess);
            }

            float sourceHeight = _capsule.Height;
            float sourceRadius = _capsule.Radius;
            float targetHeight = capsuleSize.height;
            float targetRadius = capsuleSize.radius;
            float progress = 0f;

            if (setTargetPoseAt >= 1f) setTargetPoseAt = 1f;

            while (enabled && !cancellationToken.IsCancellationRequested && changeId == _lastPoseChangeId) {
                progress = Mathf.Clamp01(progress + _timeSource.DeltaTime / duration);

                float linearHeight = Mathf.Lerp(sourceHeight, targetHeight, progress);
                float linearRadius = Mathf.Lerp(sourceRadius, targetRadius, progress);

                _capsule.Height = linearHeight;
                _capsule.Radius = linearRadius;

                if (sourcePose != targetPose && targetPose == TargetPose && progress >= setTargetPoseAt) {
                    sourcePose = _currentPose;
                    _currentPose = targetPose;
                    OnPoseChanged.Invoke(_currentPose, sourcePose);
                }

                if (progress >= 1f) return !checkCondition || canContinue.IsMatch(_characterAccess);
                if (checkCondition && !canContinue.IsMatch(_characterAccess)) return false;

                await UniTask.Yield();
            }

            return false;
        }

        public void StopPoseChange() {
            ++_lastPoseChangeId;
        }

        private void SetPose(CharacterPoseType targetPose, bool forceNotify = false) {
            if (!enabled) return;

            ++_lastPoseChangeId;

            var sourcePose = _currentPose;
            TargetPose = targetPose;
            _currentPose = targetPose;

            if (forceNotify || sourcePose != targetPose) {
                OnPoseChanged.Invoke(targetPose, sourcePose);
            }
        }
    }

}
