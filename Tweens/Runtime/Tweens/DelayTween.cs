using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Tick.Core;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.Tweens {

    [Serializable]
    public sealed class DelayTween : ITween {

        [Min(0f)] public float duration;

        private ITimeSource _timeSource;

        private float _progress;
        private float _progressDirection = 1f;

        public void Initialize(MonoBehaviour owner) {
            _timeSource = TimeSources.Get(PlayerLoopStage.Update);
        }

        public void DeInitialize() { }

        public async UniTask Play(CancellationToken token) {
            if (HasReachedTargetProgress()) return;

            if (duration <= 0f) {
                _progress = Mathf.Clamp01(_progressDirection);
                return;
            }

            while (!token.IsCancellationRequested) {
                float progressDelta = _progressDirection * _timeSource.DeltaTime / duration;
                _progress = Mathf.Clamp01(_progress + progressDelta);

                if (HasReachedTargetProgress()) break;

                await UniTask.Yield();
            }
        }

        public void Wind(bool reportProgress = true) {
            _progress = 1f;
        }

        public void Rewind(bool reportProgress = true) {
            _progress = 0f;
        }

        public void Invert(bool isInverted) {
            _progressDirection = isInverted ? -1f : 1f;
        }

        private bool HasReachedTargetProgress() {
            return _progressDirection > 0f && _progress >= 1f ||
                   _progressDirection < 0f && _progress <= 0f;
        }
    }

}
