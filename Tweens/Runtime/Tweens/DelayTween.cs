using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.Tweens {

    [Serializable]
    public sealed class DelayTween : ITween {

        [Min(0f)] public float duration;

        private float _progress;
        private float _progressDirection = 1f;

        public void Initialize(MonoBehaviour owner) { }

        public void DeInitialize() { }

        public async UniTask Play(CancellationToken token) {
            if (HasReachedTargetProgress()) return;

            if (duration <= 0f) {
                _progress = Mathf.Clamp01(_progressDirection);
                return;
            }

            while (!token.IsCancellationRequested) {
                float progressDelta = _progressDirection * Time.deltaTime / duration;
                _progress = Mathf.Clamp01(_progress + progressDelta);

                if (HasReachedTargetProgress()) break;

                await UniTask.Yield();
            }
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

        public void ResetProgress() {
            _progress = 0f;
        }

        private bool HasReachedTargetProgress() {
            return _progressDirection > 0f && _progress >= 1f ||
                   _progressDirection < 0f && _progress <= 0f;
        }
    }

}
