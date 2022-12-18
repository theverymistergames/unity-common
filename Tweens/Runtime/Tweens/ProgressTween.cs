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

        [SerializeField] [Min(0f)] private float _duration;

        [Header("Easing")]
        [SerializeField] private EasingType _easingType = EasingType.Linear;
        [SerializeField] private bool _useCustomEasingCurve;
        [SerializeField] private AnimationCurve _customEasingCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [SerializeReference] [SubclassSelector]
        private ITweenProgressAction[] _actions;

        private AnimationCurve _easingCurve;
        private float _progress;
        private float _progressDirection = 1f;

        public void Initialize(MonoBehaviour owner) {
            _easingCurve = _useCustomEasingCurve ? _customEasingCurve : _easingType.ToAnimationCurve();

            for (int i = 0; i < _actions.Length; i++) {
                _actions[i].Initialize(owner);
            }
        }

        public void DeInitialize() {
            for (int i = 0; i < _actions.Length; i++) {
                _actions[i].DeInitialize();
            }
        }

        public async UniTask Play(CancellationToken token) {
            if (HasReachedTargetProgress()) return;

            if (_duration <= 0f) {
                _progress = Mathf.Clamp01(_progressDirection);
                ReportProgress();
                return;
            }

            while (!token.IsCancellationRequested) {
                float progressDelta = _progressDirection * Time.deltaTime / _duration;
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

            for (int i = 0; i < _actions.Length; i++) {
                _actions[i].OnProgressUpdate(value);
            }
        }

        private bool HasReachedTargetProgress() {
            return _progressDirection > 0f && _progress >= 1f ||
                   _progressDirection < 0f && _progress <= 0f;
        }
    }

}
