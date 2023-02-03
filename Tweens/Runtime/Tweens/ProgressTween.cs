using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using MisterGames.Tweens.Core;
using Tweens.Easing;
using UnityEngine;

namespace MisterGames.Tweens {

    [Serializable]
    public sealed class ProgressTween : ITween {

        [Min(0f)] public float duration;

        [Header("Easing")]
        public EasingType easingType = EasingType.Linear;
        public bool useCustomEasingCurve;
        public AnimationCurve customEasingCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [SerializeReference] [SubclassSelector] public ITweenProgressAction[] actions;

        private AnimationCurve _easingCurve;
        private float _progress;
        private float _progressDirection = 1f;

        public void Initialize(MonoBehaviour owner) {
            _easingCurve = useCustomEasingCurve ? customEasingCurve : easingType.ToAnimationCurve();

            for (int i = 0; i < actions.Length; i++) {
                actions[i].Initialize(owner);
            }
        }

        public void DeInitialize() {
            for (int i = 0; i < actions.Length; i++) {
                actions[i].DeInitialize();
            }
        }

        public async UniTask Play(CancellationToken token) {
            if (HasReachedTargetProgress()) return;

            if (duration <= 0f) {
                _progress = Mathf.Clamp01(_progressDirection);
                ReportProgress();
                return;
            }

            while (!token.IsCancellationRequested) {
                float progressDelta = _progressDirection * Time.deltaTime / duration;
                _progress = Mathf.Clamp01(_progress + progressDelta);

                ReportProgress();

                if (HasReachedTargetProgress()) break;

                await UniTask.Yield();
            }
        }

        public void Wind() {
            _progress = 1f;
            ReportProgress();
        }

        public void Rewind() {
            _progress = 0f;
            ReportProgress();
        }

        public void Invert(bool isInverted) {
            _progressDirection = isInverted ? -1f : 1f;
        }

        public void ResetProgress() {
            _progress = 0f;
        }

        private void ReportProgress() {
            float value = Mathf.Clamp01(_easingCurve.Evaluate(_progress));

            for (int i = 0; i < actions.Length; i++) {
                actions[i].OnProgressUpdate(value);
            }
        }

        private bool HasReachedTargetProgress() {
            return _progressDirection > 0f && _progress >= 1f ||
                   _progressDirection < 0f && _progress <= 0f;
        }
    }

}
