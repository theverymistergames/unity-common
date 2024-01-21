using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Tweens.Core2 {

    [Serializable]
    public sealed class TweenHandle {

        [SerializeField] [Range(0f, 1f)] private float _progress;
        [SerializeField] private float _speed = 1f;
        [SerializeField] private YoyoMode _yoyo;
        [SerializeField] private bool _loop;
        [SerializeReference] [SubclassSelector] private ITween _tween;

        public ITween Tween { get => _tween; set => SetTween(value); }

        public float Duration => GetDuration(forceRecalculate: false);
        public float Timer => _progress * _duration;

        public float Progress { get => _progress; set => SetProgress(value); }
        public float Speed { get => _speed; set => SetSpeed(value); }

        public YoyoMode Yoyo { get => _yoyo; set => _yoyo = value; }
        public bool Loop { get => _loop; set => _loop = value; }

        private CancellationTokenSource _cts;
        private float _duration;
        private byte _trackProgressVersion;
        private bool _isDurationSet;
        private bool _needRecalculateDuration;

        public async UniTask Play<T>(
            T data,
            ProgressCallback<T> progressCallback = null,
            CancellationToken cancellationToken = default
        ) {
            Stop();

            // First play must not recalculate duration, if it was previously calculated.
            float duration = GetDuration(forceRecalculate: _needRecalculateDuration);
            _needRecalculateDuration = true;

            if (duration > 0f) {
                _cts = new CancellationTokenSource();
                cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token).Token;
            }

            while (!cancellationToken.IsCancellationRequested) {
                TrackProgress(data, progressCallback, cancellationToken).Forget();
                if (_tween != null) await _tween.Play(duration, _progress, _speed, cancellationToken);

                if (cancellationToken.IsCancellationRequested) break;

                if (_speed > 0f && _yoyo == YoyoMode.End ||
                    _speed < 0f && _yoyo == YoyoMode.Start
                ) {
                    _speed = -_speed;
                    continue;
                }

                if (_loop) {
                    if (_speed > 0f) {
                        if (_yoyo == YoyoMode.Start) _speed = -_speed;
                        else _progress = 0f;
                    }
                    else if (_speed < 0f) {
                        if (_yoyo == YoyoMode.End) _speed = -_speed;
                        else _progress = 1f;
                    }
                    continue;
                }

                break;
            }
        }

        public async UniTask Play(
            ProgressCallback progressCallback = null,
            CancellationToken cancellationToken = default
        ) {
            Stop();

            // First play must not recalculate duration, if it was previously calculated.
            float duration = GetDuration(forceRecalculate: _needRecalculateDuration);
            _needRecalculateDuration = true;

            if (duration > 0f) {
                _cts = new CancellationTokenSource();
                cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token).Token;
            }

            while (!cancellationToken.IsCancellationRequested) {
                TrackProgress(progressCallback, cancellationToken).Forget();
                if (_tween != null) await _tween.Play(duration, _progress, _speed, cancellationToken);

                if (cancellationToken.IsCancellationRequested) break;

                if (_speed > 0f && _yoyo == YoyoMode.End ||
                    _speed < 0f && _yoyo == YoyoMode.Start
                   ) {
                    _speed = -_speed;
                    continue;
                }

                if (_loop) {
                    if (_speed > 0f) {
                        if (_yoyo == YoyoMode.Start) _speed = -_speed;
                        else _progress = 0f;
                    }
                    else if (_speed < 0f) {
                        if (_yoyo == YoyoMode.End) _speed = -_speed;
                        else _progress = 1f;
                    }
                    continue;
                }

                break;
            }
        }

        public void Stop() {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private async UniTask TrackProgress<T>(
            T data,
            ProgressCallback<T> progressCallback = null,
            CancellationToken cancellationToken = default
        ) {
            byte version = ++_trackProgressVersion;

            if (_duration <= 0f) {
                float oldProgress = _progress;
                _progress = _speed > 0f ? 1f : 0f;
                if (!oldProgress.IsNearlyEqual(_progress)) progressCallback?.Invoke(data, _progress);
                return;
            }

            var timeSource = TimeSources.Get(PlayerLoopStage.Update);

            while (!cancellationToken.IsCancellationRequested && version == _trackProgressVersion) {
                float dt = timeSource.DeltaTime;

                float oldProgress = _progress;
                _progress = Mathf.Clamp01(_progress + dt * _speed / _duration);
                if (!oldProgress.IsNearlyEqual(_progress)) progressCallback?.Invoke(data, _progress);

                if (_speed > 0 && _progress >= 1f || _speed < 0 && _progress <= 0f) {
                    break;
                }

                await UniTask.Yield();
            }
        }

        private async UniTask TrackProgress(
            ProgressCallback progressCallback = null,
            CancellationToken cancellationToken = default
        ) {
            byte version = ++_trackProgressVersion;

            if (_duration <= 0f) {
                float oldProgress = _progress;
                _progress = _speed > 0f ? 1f : 0f;
                if (!oldProgress.IsNearlyEqual(_progress)) progressCallback?.Invoke(_progress);
                return;
            }

            var timeSource = TimeSources.Get(PlayerLoopStage.Update);

            while (!cancellationToken.IsCancellationRequested && version == _trackProgressVersion) {
                float dt = timeSource.DeltaTime;

                float oldProgress = _progress;
                _progress = Mathf.Clamp01(_progress + dt * _speed / _duration);
                if (!oldProgress.IsNearlyEqual(_progress)) progressCallback?.Invoke(_progress);

                if (_speed > 0 && _progress >= 1f || _speed < 0 && _progress <= 0f) {
                    break;
                }

                await UniTask.Yield();
            }
        }

        private void SetTween(ITween tween) {
            _tween = tween;
            _isDurationSet = false;
        }

        private void SetProgress(float value) {
            Stop();
            _progress = Mathf.Clamp01(value);
        }

        private void SetSpeed(float value) {
            Stop();
            _speed = value;
        }

        private float GetDuration(bool forceRecalculate) {
            if (_isDurationSet && !forceRecalculate) return _duration;

            _duration = _tween?.CreateDuration() ?? 0f;
            _isDurationSet = true;
            return _duration;
        }
    }

}
