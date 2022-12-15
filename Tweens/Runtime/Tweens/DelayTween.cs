using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Tick.Core;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.Tweens {

    [Serializable]
    public sealed class DelayTween : ITween, IUpdate {

        [SerializeField] private PlayerLoopStage _playerLoopStage = PlayerLoopStage.Update;
        [SerializeField] [Min(0f)] private float _duration;

        private float _progress;
        private float _progressDirection = 1f;

        private readonly AutoResetUniTaskCompletionSource _completionSource = AutoResetUniTaskCompletionSource.Create();
        private CancellationToken _token;

        public void Initialize(MonoBehaviour owner) { }

        public void DeInitialize() {
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
                return;
            }


            TimeSources.Get(_playerLoopStage).Subscribe(this);
            await _completionSource.Task;
        }

        public void Wind() {
            _progress = 1f;
        }

        public void Rewind() {
            _progress = 0f;
        }

        public void Invert(bool isInverted) {
            _progressDirection = isInverted ? -1f : 1f;
        }

        public void OnUpdate(float dt) {
            if (_token.IsCancellationRequested) {
                OnFinish();
                return;
            }

            _progress = Mathf.Clamp01(_progress + _progressDirection * dt / _duration);

            if (HasReachedTargetProgress()) {
                OnFinish();
            }
        }

        private void OnFinish() {
            TimeSources.Get(_playerLoopStage).Unsubscribe(this);
            _completionSource.TrySetResult();
        }

        private bool HasReachedTargetProgress() {
            return _progressDirection > 0f && _progress >= 1f ||
                   _progressDirection < 0f && _progress <= 0f;
        }
    }

}
