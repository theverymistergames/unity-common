using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using MisterGames.Tick.Core;
using MisterGames.Tweens.Core;
using Tweens.Easing;
using UnityEngine;

namespace MisterGames.Tweens {

    [Serializable]
    public sealed class ProgressTween : ITween, IUpdate {

        [SerializeField] private PlayerLoopStage _playerLoopStage = PlayerLoopStage.Update;
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

        private readonly AutoResetUniTaskCompletionSource _completionSource = AutoResetUniTaskCompletionSource.Create();
        private CancellationToken _token;

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

            TimeSources.Get(_playerLoopStage).Unsubscribe(this);
            _completionSource.TrySetCanceled(_token);
        }

        public async UniTask Play(CancellationToken token) {
            _token = token;

            if (HasReachedTargetProgress()) {
                return;
            }

            if (_duration <= 0f) {
                _progress = Mathf.Clamp01(_progressDirection);
                ReportProgress();
                return;
            }

            TimeSources.Get(_playerLoopStage).Subscribe(this);
            await _completionSource.Task;
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

        public void OnUpdate(float dt) {
            if (_token.IsCancellationRequested) {
                OnFinish();
                return;
            }

            float progressDelta = _duration <= 0f ? _progressDirection : _progressDirection * dt / _duration;
            _progress = Mathf.Clamp01(_progress + progressDelta);

            ReportProgress();

            if (HasReachedTargetProgress()) OnFinish();
        }

        private void OnFinish() {
            TimeSources.Get(_playerLoopStage).Unsubscribe(this);
            _completionSource.TrySetResult();
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
