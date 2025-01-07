using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Character.Input;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Capsule {

    public sealed class CharacterPoseGraphPipeline : MonoBehaviour, IActorComponent {

        [EmbeddedInspector]
        [SerializeField] private CharacterPoseGraph _poseGraph;

        private IActor _actor;
        private CharacterPosePipeline _pose;
        private CharacterInputPipeline _input;
        private CancellationTokenSource _enableCts;
        private CancellationTokenSource _poseChangeCts;
        private byte _poseChangeId;
        private float _startTime;

        public void OnAwake(IActor actor) {
            _actor = actor;
            _input = actor.GetComponent<CharacterInputPipeline>();
            _pose = actor.GetComponent<CharacterPosePipeline>();
        }

        private void Start() {
            _pose.CurrentCapsuleSize = _poseGraph.initialPose.CapsuleSize;
            _pose.CurrentPose = _poseGraph.initialPose;
        }

        private void OnEnable() {
            _startTime = Time.time;
                
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

            ChangePose(_poseGraph.crouchPose, _enableCts.Token).Forget();
        }

        private void OnCrouchReleased() {
            if (!enabled) return;

            ChangePose(_poseGraph.standPose, _enableCts.Token).Forget();
        }

        private void OnCrouchToggled() {
            if (!enabled) return;

            var nextPose = _pose.TargetPose == _poseGraph.crouchPose
                ? _poseGraph.standPose
                : _poseGraph.crouchPose;

            ChangePose(nextPose, _enableCts.Token).Forget();
        }

        private async UniTask ChangePose(CharacterPose targetPose, CancellationToken cancellationToken = default) {
            if (!enabled) return;

            byte id = ++_poseChangeId;
            var sourcePose = _pose.CurrentPose;

            // Try to use transition for last target pose instead of current pose:
            // if last pose change did not cause actual pose type change.
            if (sourcePose == targetPose) sourcePose = _pose.TargetPose;

            if (!TryGetTransition(sourcePose, targetPose, out var transition)) {
                while (!cancellationToken.IsCancellationRequested && id == _poseChangeId && 
                       !TryGetTransition(sourcePose, targetPose, out transition)
                ) {
                    await UniTask.Delay(TimeSpan.FromSeconds(_poseGraph.retryDelay), cancellationToken: cancellationToken)
                        .SuppressCancellationThrow();
                }    
            }
            
            if (cancellationToken.IsCancellationRequested || id != _poseChangeId || transition == null) return;
            
            var targetCapsuleSize = targetPose.CapsuleSize;

            float sourceHeight = sourcePose.CapsuleSize.height;
            float targetHeight = targetCapsuleSize.height;
            float currentHeight = _pose.CurrentCapsuleSize.height;

            float progress = GetTransitionProgressDone(sourceHeight, targetHeight, currentHeight);
            float duration = GetTransitionDurationLeft(transition.Duration, progress);
            float setPoseAt = GetTransitionPoseSetMark(transition.SetPoseAt, progress);

            transition.Action?.Apply(_actor, cancellationToken).Forget();

            await _pose.ChangePose(targetPose, targetCapsuleSize, duration, setPoseAt, cancellationToken);
        }

        private bool TryGetTransition(
            CharacterPose sourcePose,
            CharacterPose targetPose,
            out CharacterPoseTransition transition
        ) {
            var transitions = _poseGraph.transitions;

            for (int i = 0; i < transitions.Length; i++) {
                var t = transitions[i];

                if (t.SourcePose == sourcePose &&
                    t.TargetPose == targetPose &&
                    (t.Condition == null || t.Condition.IsMatch(_actor, _startTime))
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
