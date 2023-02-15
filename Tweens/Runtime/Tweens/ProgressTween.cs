using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using MisterGames.Common.Easing;
using MisterGames.Tick.Core;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.Tweens {

    [Serializable]
    public sealed class ProgressTween : ITween {

        [Min(0f)] public float duration;

        [Header("Easing")]
        public EasingType easingType = EasingType.Linear;
        public bool useCustomEasingCurve;
        public AnimationCurve customEasingCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [SerializeReference] [SubclassSelector] public ITweenProgressAction action;

        private ITimeSource _timeSource;

        private AnimationCurve _easingCurve;
        private float _progress;
        private float _progressDirection = 1f;

        public void Initialize(MonoBehaviour owner) {
            _timeSource = TimeSources.Get(PlayerLoopStage.Update);

            _easingCurve = useCustomEasingCurve ? customEasingCurve : easingType.ToAnimationCurve();

            action.Initialize(owner);
        }

        public void DeInitialize() {
            action.DeInitialize();
        }

        public async UniTask Play(CancellationToken token) {
            if (HasReachedTargetProgress()) return;

            if (duration <= 0f) {
                _progress = Mathf.Clamp01(_progressDirection);
                ReportProgress();
                return;
            }

            while (!token.IsCancellationRequested) {
                float progressDelta = _progressDirection * _timeSource.DeltaTime / duration;
                _progress = Mathf.Clamp01(_progress + progressDelta);

                ReportProgress();

                if (HasReachedTargetProgress()) break;

                await UniTask.Yield();
            }
        }

        public void Wind(bool reportProgress = true) {
            _progress = 1f;
            if (reportProgress) ReportProgress();
        }

        public void Rewind(bool reportProgress = true) {
            _progress = 0f;
            if (reportProgress) ReportProgress();
        }

        public void Invert(bool isInverted) {
            _progressDirection = isInverted ? -1f : 1f;
        }

        private void ReportProgress() {
            float value = Mathf.Clamp01(_easingCurve.Evaluate(_progress));
            action.OnProgressUpdate(value);
        }

        private bool HasReachedTargetProgress() {
            return _progressDirection > 0f && _progress >= 1f ||
                   _progressDirection < 0f && _progress <= 0f;
        }
    }

}
