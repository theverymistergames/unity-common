using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.Tweens {

    [Serializable]
    public sealed class DelayTween : ITween {

        [SerializeField] [Min(0f)] private float _duration;

        private float _progress;
        private float _progressDirection = 1f;

        public void Initialize(MonoBehaviour owner) { }

        public void DeInitialize() { }

        public async UniTask Play(CancellationToken token) {
            if (HasReachedTargetProgress()) return;

            if (_duration <= 0f) {
                _progress = Mathf.Clamp01(_progressDirection);
                return;
            }

            while (!token.IsCancellationRequested) {
                float progressDelta = _progressDirection * Time.deltaTime / _duration;
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
