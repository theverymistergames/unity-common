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
        [SerializeReference] [SubclassSelector] public ITweenProgressAction action;

        private ITimeSource _timeSource;

        private float _progress;
        private float _progressDirection = 1f;

        public void Initialize(MonoBehaviour owner) {
            _timeSource = TimeSources.Get(PlayerLoopStage.Update);

            action.Initialize(owner);
        }

        public void DeInitialize() {
            action.DeInitialize();
        }

        public async UniTask Play(CancellationToken token) {
            if (HasReachedTargetProgress()) return;

            action.Start();

            if (duration <= 0f) {
                _progress = Mathf.Clamp01(_progressDirection);
                ReportProgress();
                action.Finish();
                return;
            }

            while (!token.IsCancellationRequested) {
                float progressDelta = _progressDirection * _timeSource.DeltaTime / duration;
                _progress = Mathf.Clamp01(_progress + progressDelta);

                ReportProgress();

                if (HasReachedTargetProgress()) break;

                await UniTask.Yield();
            }

            action.Finish();
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
            action.OnProgressUpdate(Mathf.Clamp01(curve.Evaluate(_progress)));
        }

        private bool HasReachedTargetProgress() {
            return _progressDirection > 0f && _progress >= 1f ||
                   _progressDirection < 0f && _progress <= 0f;
        }
    }

}
