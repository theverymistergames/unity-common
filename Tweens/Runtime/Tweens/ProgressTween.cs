using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using MisterGames.Tick.Core;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.Tweens {

    [Serializable]
    public sealed class ProgressTween : ITween {

        [Min(0f)] public float duration;
        public AnimationCurve curve;
        [SerializeReference] [SubclassSelector] public ITweenProgressCallback action;

        public float Progress => _progress;
        public float T => _progressT;

        private float _progressDirection = 1f;
        private float _progress;
        private float _progressT;

        public void Initialize(MonoBehaviour owner) { }

        public void DeInitialize() { }

        public async UniTask Play(CancellationToken token) {
            if (HasReachedTargetProgress()) return;

            if (duration <= 0f) {
                _progress = Mathf.Clamp01(_progressDirection);
                _progressT = curve.Evaluate(_progress);

                action.OnProgressUpdate(_progressT);

                return;
            }

            var timeSource = TimeSources.Get(PlayerLoopStage.Update);

            while (!token.IsCancellationRequested) {
                float progressDelta = _progressDirection * timeSource.DeltaTime / duration;

                _progress = Mathf.Clamp01(_progress + progressDelta);
                _progressT = curve.Evaluate(_progress);

                action.OnProgressUpdate(_progressT);

                if (HasReachedTargetProgress()) break;

                await UniTask.Yield();
            }
        }

        public void Wind(bool reportProgress = true) {
            _progress = 1f;
            _progressT = curve.Evaluate(_progress);

            if (!reportProgress) return;

            action.OnProgressUpdate(_progressT);
        }

        public void Rewind(bool reportProgress = true) {
            _progress = 0f;
            _progressT = curve.Evaluate(_progress);

            if (!reportProgress) return;

            action.OnProgressUpdate(_progressT);
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
