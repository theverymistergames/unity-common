using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Character.Core;
using MisterGames.Character.Input;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Capsule {

    public class CharacterPoseGraphPipeline : CharacterPipelineBase, IActorComponent, ICharacterPoseGraphPipeline {

        [EmbeddedInspector]
        [SerializeField] private CharacterPoseGraph _poseGraph;

        [SerializeField] private CharacterPose _crouchPose;
        [SerializeField] private CharacterPose _standPose;

        public override bool IsEnabled { get => enabled; set => enabled = value; }

        private IActor _actor;
        private ICharacterPosePipeline _pose;
        private ICharacterInputPipeline _input;
        private CancellationTokenSource _enableCts;
        private CancellationTokenSource _poseChangeCts;

        void IActorComponent.OnAwakeActor(IActor actor) {
            _actor = actor;
            _input = actor.GetComponent<ICharacterInputPipeline>();
            _pose = actor.GetComponent<ICharacterPosePipeline>();
        }

        private void Start() {
            _pose.CurrentCapsuleSize = _poseGraph.InitialPose.CapsuleSize;
            _pose.CurrentPose = _poseGraph.InitialPose;
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

            ChangePose(_crouchPose, _enableCts.Token).Forget();
        }

        private void OnCrouchReleased() {
            if (!enabled) return;

            ChangePose(_standPose, _enableCts.Token).Forget();
        }

        private void OnCrouchToggled() {
            if (!enabled) return;

            var nextPose = _pose.TargetPose == _crouchPose
                ? _standPose
                : _crouchPose;

            ChangePose(nextPose, _enableCts.Token).Forget();
        }

        private async UniTask ChangePose(CharacterPose targetPose, CancellationToken cancellationToken = default) {
            if (!enabled) return;

            var sourcePose = _pose.CurrentPose;

            // Try to use transition for last target pose instead of current pose:
            // if last pose change did not cause actual pose type change.
            if (sourcePose == targetPose) sourcePose = _pose.TargetPose;

            if (!TryGetTransition(sourcePose, targetPose, out var transition)) {
                return;
            }

            var targetCapsuleSize = targetPose.CapsuleSize;

            float sourceHeight = sourcePose.CapsuleSize.height;
            float targetHeight = targetCapsuleSize.height;
            float currentHeight = _pose.CurrentCapsuleSize.height;

            float progress = GetTransitionProgressDone(sourceHeight, targetHeight, currentHeight);
            float duration = GetTransitionDurationLeft(transition.Duration, progress);
            float setPoseAt = GetTransitionPoseSetMark(transition.SetPoseAt, progress);

            _poseChangeCts?.Cancel();
            _poseChangeCts?.Dispose();
            _poseChangeCts = new CancellationTokenSource();
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _poseChangeCts.Token);

            transition.Action?.Apply(_actor, cancellationToken).Forget();

            await _pose.ChangePose(targetPose, targetCapsuleSize, duration, setPoseAt, linkedCts.Token);
        }

        private bool TryGetTransition(
            CharacterPose sourcePose,
            CharacterPose targetPose,
            out CharacterPoseTransition transition
        ) {
            var transitions = _poseGraph.Transitions;

            for (int i = 0; i < transitions.Count; i++) {
                var t = transitions[i];

                if (t.SourcePose == sourcePose &&
                    t.TargetPose == targetPose &&
                    (t.Condition == null || t.Condition.IsMatch(_actor))
                ) {
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
            if (progressDone >= 1f || progressDone >= setPoseAt) return 0f;
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
