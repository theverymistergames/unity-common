using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Character.Input;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Capsule {

    public class CharacterPoseGraphPipeline : CharacterPipelineBase, ICharacterPoseGraphPipeline {

        [SerializeField] private CharacterAccess _characterAccess;

        [EmbeddedInspector]
        [SerializeField] private CharacterPoseGraph _poseGraph;

        public override bool IsEnabled { get => enabled; set => enabled = value; }

        private ICharacterPosePipeline _pose;
        private ICharacterInputPipeline _input;
        private CancellationTokenSource _enableCts;
        private CancellationTokenSource _poseChangeCts;

        private void Awake() {
            _input = _characterAccess.GetPipeline<ICharacterInputPipeline>();
            _pose = _characterAccess.GetPipeline<ICharacterPosePipeline>();
        }

        private void OnEnable() {
            _enableCts?.Cancel();
            _enableCts?.Dispose();
            _enableCts = new CancellationTokenSource();

            _input.OnCrouchPressed -= OnCrouchPressed;
            _input.OnCrouchPressed += OnCrouchPressed;

            _input.OnCrouchReleased -= OnCrouchReleased;
            _input.OnCrouchReleased += OnCrouchReleased;

            _input.OnCrouchToggled -= OnCrouchToggled;
            _input.OnCrouchToggled += OnCrouchToggled;
        }

        private void OnDisable() {
            _enableCts?.Cancel();
            _enableCts?.Dispose();
            _enableCts = null;

            _input.OnCrouchPressed -= OnCrouchPressed;
            _input.OnCrouchReleased -= OnCrouchReleased;
            _input.OnCrouchToggled -= OnCrouchToggled;
        }

        private void OnCrouchPressed() {
            if (!enabled) return;

            ChangePose(CharacterPoseType.Crouch, _enableCts.Token).Forget();
        }

        private void OnCrouchReleased() {
            if (!enabled) return;

            ChangePose(CharacterPoseType.Stand, _enableCts.Token).Forget();
        }

        private void OnCrouchToggled() {
            if (!enabled) return;

            var nextPose = _pose.TargetPose == CharacterPoseType.Crouch
                ? CharacterPoseType.Stand
                : CharacterPoseType.Crouch;

            ChangePose(nextPose, _enableCts.Token).Forget();
        }

        private async UniTask ChangePose(CharacterPoseType targetPose, CancellationToken cancellationToken = default) {
            if (!enabled) return;

            var sourcePose = _pose.CurrentPose;

            // Try to use transition for last target pose instead of current pose:
            // if last pose change did not cause actual pose type change.
            if (sourcePose == targetPose) sourcePose = _pose.TargetPose;

            if (!TryGetTransition(sourcePose, targetPose, out var transition)) {
                return;
            }

            var sourcePoseSettings = _poseGraph.GetPoseSettings(sourcePose);
            var targetPoseSettings = _poseGraph.GetPoseSettings(targetPose);
            var targetCapsuleSize = targetPoseSettings.capsuleSize;

            float sourceHeight = sourcePoseSettings.capsuleSize.height;
            float targetHeight = targetPoseSettings.capsuleSize.height;
            float currentHeight = _pose.CurrentCapsuleSize.height;

            float progress = GetTransitionProgressDone(sourceHeight, targetHeight, currentHeight);
            float duration = GetTransitionDurationLeft(transition.Duration, progress);
            float setPoseAt = GetTransitionPoseSetMark(transition.SetPoseAt, progress);

            _poseChangeCts?.Cancel();
            _poseChangeCts?.Dispose();
            _poseChangeCts = new CancellationTokenSource();
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _poseChangeCts.Token);

            transition.Action?.Apply(_characterAccess, linkedCts.Token).Forget();

            await _pose.ChangePose(targetPose, targetCapsuleSize, duration, setPoseAt, linkedCts.Token);
        }

        private bool TryGetTransition(
            CharacterPoseType sourcePose,
            CharacterPoseType targetPose,
            out CharacterPoseTransition transition
        ) {
            var transitions = _poseGraph.GetPoseSettings(sourcePose).transitions;

            for (int i = 0; i < transitions.Length; i++) {
                var (pose, t) = transitions[i];
                if (pose == targetPose && (t.Condition == null || t.Condition.IsMatch(_characterAccess))) {
                    transition = t;
                    return true;
                }
            }

            transition = default;
            return false;
        }

        private static float GetTransitionDurationLeft(float totalDuration, float progressDone) {
            return totalDuration * (1f - progressDone);
        }

        private static float GetTransitionPoseSetMark(float setPoseAt, float progressDone) {
            if (setPoseAt <= progressDone) return 0f;
            if (setPoseAt >= 1f) return 1f;

            return (setPoseAt - progressDone) / (1f - progressDone);
        }

        private static float GetTransitionProgressDone(float sourceHeight, float targetHeight, float currentHeight) {
            return sourceHeight.IsNearlyEqual(targetHeight)
                ? 0f
                : Mathf.Clamp01((currentHeight - sourceHeight) / (targetHeight - sourceHeight));
        }
    }

}
