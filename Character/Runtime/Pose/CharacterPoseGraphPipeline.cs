using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Character.Input;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Pose {

    public sealed class CharacterPoseGraphPipeline : CharacterPipelineBase, ICharacterPoseGraphPipeline {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] [Min(0f)] private float _retryChangePoseDelay;

        [EmbeddedInspector]
        [SerializeField] private CharacterPoseSettings poseSettings;

        public override bool IsEnabled { get => enabled; set => enabled = value; }

        private ICharacterCapsulePipeline _capsule;
        private ICharacterInputPipeline _input;
        private CancellationTokenSource _enableCts;

        private byte _lastRetryChangePoseId;

        private void Awake() {
            _input = _characterAccess.GetPipeline<ICharacterInputPipeline>();
            _capsule = _characterAccess.GetPipeline<ICharacterCapsulePipeline>();
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

        public CharacterCapsuleSize GetCapsuleSize(CharacterPoseType pose) {
            return pose switch {
                CharacterPoseType.Stand => poseSettings.stand,
                CharacterPoseType.Crouch => poseSettings.crouch,
                _ => throw new ArgumentOutOfRangeException(nameof(pose), pose, null)
            };
        }

        public float GetDefaultTransitionDuration(CharacterPoseType targetPose) {
            return TryGetTransition(targetPose, out var transition)
                ? GetTransitionDurationLeftover(targetPose, transition.duration)
                : 0f;
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

            var nextPose = _capsule.CurrentPose switch {
                CharacterPoseType.Stand => CharacterPoseType.Crouch,
                CharacterPoseType.Crouch => CharacterPoseType.Stand,
                _ => throw new ArgumentOutOfRangeException()
            };

            ChangePose(nextPose, _enableCts.Token).Forget();
        }

        private UniTask ChangePose(CharacterPoseType targetPose, CancellationToken cancellationToken = default) {
            if (!enabled || _capsule.TargetPose == targetPose) return default;

            StopRetryAttempts();

            if (!TryGetTransition(targetPose, out var transition)) {
                StartRetryAttempts(targetPose, cancellationToken).Forget();
                return default;
            }

            var capsuleSize = GetCapsuleSize(targetPose);
            float duration = GetTransitionDurationLeftover(targetPose, transition.duration);

            return _capsule.ChangePose(targetPose, capsuleSize, duration, transition.changePoseAt, cancellationToken);
        }

        private async UniTask StartRetryAttempts(CharacterPoseType targetPose, CancellationToken cancellationToken = default) {
            byte id = ++_lastRetryChangePoseId;

            while (!cancellationToken.IsCancellationRequested && id == _lastRetryChangePoseId) {
                bool cancelled = false;

                if (_retryChangePoseDelay > 0f) {
                    cancelled = await UniTask
                        .Delay(TimeSpan.FromSeconds(_retryChangePoseDelay), cancellationToken: cancellationToken)
                        .SuppressCancellationThrow();
                }
                else {
                    await UniTask.Yield();
                }

                if (cancelled || id != _lastRetryChangePoseId) break;

                if (TryGetTransition(targetPose, out _)) {
                    await ChangePose(targetPose, cancellationToken);
                }
            }
        }

        private void StopRetryAttempts() {
            _lastRetryChangePoseId++;
        }

        private bool TryGetTransition(CharacterPoseType targetPose, out CharacterPoseTransition transition) {
            var transitions = poseSettings.transitions;

            for (int i = 0; i < transitions.Length; i++) {
                var t = transitions[i];
                if (t.targetPose == targetPose && (t.condition == null || t.condition.IsMatch(_characterAccess))) {
                    transition = t;
                    return true;
                }
            }

            transition = default;
            return false;
        }

        private float GetTransitionDurationLeftover(CharacterPoseType targetPose, float totalDuration) {
            float sourceHeight = GetCapsuleSize(_capsule.CurrentPose).colliderHeight;
            float targetHeight = GetCapsuleSize(targetPose).colliderHeight;

            if (sourceHeight.IsNearlyEqual(targetHeight)) return 0f;

            return (targetHeight - _capsule.CurrentHeight) / (targetHeight - sourceHeight) * totalDuration;
        }
    }

}
