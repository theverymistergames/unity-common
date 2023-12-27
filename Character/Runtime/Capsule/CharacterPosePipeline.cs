using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Capsule {

    public sealed class CharacterPosePipeline : CharacterPipelineBase, ICharacterPosePipeline {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] private PlayerLoopStage _playerLoopStage = PlayerLoopStage.Update;

        public event OnPoseChanged OnPoseChanged = delegate {  };

        public CharacterPose CurrentPose { get => _currentPose; set => SetPose(value); }
        public CharacterPose TargetPose { get; private set; }

        public CharacterCapsuleSize CurrentCapsuleSize { get => _capsule.CapsuleSize; set => _capsule.CapsuleSize = value; }
        public CharacterCapsuleSize TargetCapsuleSize { get; private set; }

        public override bool IsEnabled { get => enabled; set => enabled = value; }

        private ICharacterCapsulePipeline _capsule;
        private ITimeSource _timeSource;

        private CharacterPose _currentPose;
        private byte _lastPoseChangeId;

        private void Awake() {
            _timeSource = TimeSources.Get(_playerLoopStage);
            _capsule = _characterAccess.GetPipeline<ICharacterCapsulePipeline>();
        }

        public async UniTask ChangePose(
            CharacterPose targetPose,
            CharacterCapsuleSize capsuleSize,
            float duration,
            float setTargetPoseAt = 0f,
            CancellationToken cancellationToken = default
        ) {
            if (!enabled) return;

            byte changeId = ++_lastPoseChangeId;

            var sourcePose = _currentPose;
            TargetPose = targetPose;
            TargetCapsuleSize = capsuleSize;

            if (duration <= 0f) {
                _capsule.CapsuleSize = capsuleSize;

                if (sourcePose != targetPose) {
                    _currentPose = targetPose;
                    OnPoseChanged.Invoke(targetPose, sourcePose);
                }

                return;
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

                if (progress >= setTargetPoseAt && _currentPose != TargetPose) {
                    sourcePose = _currentPose;
                    _currentPose = TargetPose;
                    OnPoseChanged.Invoke(_currentPose, sourcePose);
                }

                if (progress >= 1f) return;

                await UniTask.Yield();
            }
        }

        private void SetPose(CharacterPose targetPose) {
            if (!enabled) return;

            ++_lastPoseChangeId;

            var sourcePose = _currentPose;
            TargetPose = targetPose;
            _currentPose = targetPose;

            if (sourcePose != targetPose) {
                OnPoseChanged.Invoke(targetPose, sourcePose);
            }
        }
    }

}
