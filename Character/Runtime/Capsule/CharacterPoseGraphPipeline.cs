using System;
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
        [SerializeField] private CharacterPoseSettings _poseSettings;

        public override bool IsEnabled { get => enabled; set => enabled = value; }

        private ICharacterPosePipeline _pose;
        private ICharacterInputPipeline _input;
        private CancellationTokenSource _enableCts;
        private CancellationTokenSource _poseChangeCts;

        private byte _lastPoseChangeId;

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

        public CharacterCapsuleSize GetDefaultCapsuleSize(CharacterPoseType pose) {
            return GetCapsuleSize(_poseSettings, pose);
        }

        public float GetDefaultTransitionDuration(CharacterPoseType targetPose) {
            var sourcePose = _pose.CurrentPose;

            // Try to use transition for last target pose instead of current pose
            if (sourcePose == targetPose) sourcePose = _pose.TargetPose;

            if (!TryGetTransition(_poseSettings, sourcePose, targetPose, out var transition)) return 0f;

            var currentCapsuleSize = _pose.CurrentCapsuleSize;
            var sourceCapsuleSize = GetCapsuleSize(_poseSettings, sourcePose);
            var targetCapsuleSize = GetCapsuleSize(_poseSettings, targetPose);

            float progress = GetTransitionProgress(sourceCapsuleSize.height, targetCapsuleSize.height, currentCapsuleSize.height);
            return ConvertTransitionDuration(transition.duration, progress);
        }

        private void OnCrouchPressed() {
            if (!enabled) return;

            ChangePose(CharacterPoseType.Crouch, executeTransitionAction: true, _enableCts.Token).Forget();
        }

        private void OnCrouchReleased() {
            if (!enabled) return;

            ChangePose(CharacterPoseType.Stand, executeTransitionAction: true, _enableCts.Token).Forget();
        }

        private void OnCrouchToggled() {
            if (!enabled) return;

            var nextPose = _pose.CurrentPose switch {
                CharacterPoseType.Stand => CharacterPoseType.Crouch,
                CharacterPoseType.Crouch => CharacterPoseType.Stand,
                _ => throw new ArgumentOutOfRangeException()
            };

            ChangePose(nextPose, executeTransitionAction: true, _enableCts.Token).Forget();
        }

        private async UniTask ChangePose(
            CharacterPoseType targetPose,
            bool executeTransitionAction,
            CancellationToken cancellationToken = default
        ) {
            if (!enabled) return;

            var sourcePose = _pose.CurrentPose;

            // Try to use transition for last target pose instead of current pose
            if (sourcePose == targetPose) sourcePose = _pose.TargetPose;

            if (!TryGetTransition(_poseSettings, sourcePose, targetPose, out var transition)) return;

            var currentCapsuleSize = _pose.CurrentCapsuleSize;
            var sourceCapsuleSize = GetCapsuleSize(_poseSettings, sourcePose);
            var targetCapsuleSize = GetCapsuleSize(_poseSettings, targetPose);

            float progress = GetTransitionProgress(sourceCapsuleSize.height, targetCapsuleSize.height, currentCapsuleSize.height);
            float duration = ConvertTransitionDuration(transition.duration, progress);
            float setPoseAt = ConvertTransitionPoseSetMoment(transition.setPoseAt, progress);

            _poseChangeCts?.Cancel();
            _poseChangeCts?.Dispose();
            _poseChangeCts = new CancellationTokenSource();
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _poseChangeCts.Token);

            byte id = ++_lastPoseChangeId;

            if (executeTransitionAction) transition.action?.Apply(_characterAccess, linkedCts.Token).Forget();

            bool isPoseChanged = await _pose.TryChangePose(
                targetPose,
                targetCapsuleSize,
                duration,
                setPoseAt,
                transition.condition,
                linkedCts.Token
            );

            if (isPoseChanged) return;

            // Check if next pose change called
            if (id != _lastPoseChangeId) return;

            // Execute pose change back to source pose.
            // If current pose has not been changed, then transition action is not to be executed.
            executeTransitionAction = _pose.CurrentPose != _pose.TargetPose;
            ChangePose(sourcePose, executeTransitionAction, linkedCts.Token).Forget();
        }

        private static float ConvertTransitionDuration(float totalDuration, float progress) {
            return totalDuration * (1f - progress);
        }

        private static float ConvertTransitionPoseSetMoment(float setPoseAt, float progress) {
            if (setPoseAt <= progress) return 0f;
            if (setPoseAt >= 1f) return 1f;

            return (setPoseAt - progress) / (1f - progress);
        }

        private static float GetTransitionProgress(float sourceHeight, float targetHeight, float currentHeight) {
            return sourceHeight.IsNearlyEqual(targetHeight)
                ? 0f
                : Mathf.Clamp01((currentHeight - sourceHeight) / (targetHeight - sourceHeight));
        }

        private static CharacterCapsuleSize GetCapsuleSize(CharacterPoseSettings poseSettings, CharacterPoseType pose) {
            return pose switch {
                CharacterPoseType.Stand => poseSettings.stand,
                CharacterPoseType.Crouch => poseSettings.crouch,
                _ => throw new ArgumentOutOfRangeException(nameof(pose), pose, null)
            };
        }

        private static bool TryGetTransition(
            CharacterPoseSettings poseSettings,
            CharacterPoseType sourcePose,
            CharacterPoseType targetPose,
            out CharacterPoseTransition transition
        ) {
            var transitions = poseSettings.transitions;

            for (int i = 0; i < transitions.Length; i++) {
                var t = transitions[i];
                if (t.sourcePose == sourcePose && t.targetPose == targetPose) {
                    transition = t;
                    return true;
                }
            }

            transition = default;
            return false;
        }
    }

}
