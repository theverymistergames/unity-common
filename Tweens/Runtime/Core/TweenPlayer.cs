using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Tweens {

    [Serializable]
    public sealed class TweenPlayer<TContext, TTween> : ITweenPlayer where TTween : ITween<TContext> {

        [SerializeField] [Range(0f, 1f)] private float _progress;
        [SerializeField] private float _speed = 1f;
        [SerializeField] private YoyoMode _yoyo;
        [SerializeField] private bool _loop;
        [SerializeField] private bool _invertNextPlay;
        [SerializeReference] [SubclassSelector] private TTween _tween;

        public event ProgressCallback OnProgressUpdate = delegate { };
        
        public TContext Context { get; set; }
        public TTween Tween { get => _tween; set => SetTween(value); }
        
        public float Duration => GetDuration(forceRecalculate: false);
        public float Timer => _progress * _tween?.Duration ?? 0f;
        public float Progress { get => _progress; set => SetProgress(value); }
        public float Speed { get => _speed; set => SetSpeed(value); }
        public YoyoMode Yoyo { get => _yoyo; set => _yoyo = value; }
        public bool Loop { get => _loop; set => _loop = value; }
        public bool InvertNextPlay { get => _invertNextPlay; set => _invertNextPlay = value; }

        private CancellationTokenSource _cts;
        private byte _trackProgressId;
        private PlayState _playState;
        
        private enum PlayState {
            DurationNotSet,
            DurationCalculated,
            Playing,
            Completed,
        }
        
        public async UniTask<bool> Play<T>(
            T data,
            ProgressCallback<TContext, T> progressCallback,
            float progress = -1f,
            CancellationToken cancellationToken = default
        ) {
            Stop();

            if (progress >= 0f) _progress = progress;
            
            float duration = GetDuration(forceRecalculate: _playState != PlayState.DurationCalculated);
            if (_invertNextPlay && _playState == PlayState.Playing) _speed = -_speed;
            _playState = PlayState.Playing;
            
            if (duration > 0f) {
                _cts = new CancellationTokenSource();
                cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token).Token;
            }
            
            while (!cancellationToken.IsCancellationRequested) {
                _tween?.Play(Context, duration, _progress, _speed, cancellationToken).Forget();
                await TrackProgress(data, duration, _speed, progressCallback, cancellationToken);

                if (cancellationToken.IsCancellationRequested) break;

                _progress = _speed >= 0f ? 1f : 0f;
                
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

                    await UniTask.Yield();
                    duration = GetDuration(forceRecalculate: true);
                    continue;
                }

                _playState = PlayState.Completed;
                if (_invertNextPlay) _speed = -_speed;
                
                return true;
            }

            return false;
        }

        public UniTask<bool> Play(
            ProgressCallback<TContext> progressCallback,
            float progress = -1f,
            CancellationToken cancellationToken = default
        ) {
            return Play(
                data: progressCallback,
                progressCallback: (context, callback, p, oldP) => callback.Invoke(context, p, oldP),
                progress,
                cancellationToken
            );
        }

        public UniTask<bool> Play<T>(
            T data,
            ProgressCallback<T> progressCallback,
            float progress = -1f,
            CancellationToken cancellationToken = default
        ) {
            return Play(
                data: (data, progressCallback),
                progressCallback: (_, d, p, oldP) => d.progressCallback.Invoke(d.data, p, oldP),
                progress,
                cancellationToken
            );
        }
        
        public UniTask<bool> Play(
            ProgressCallback progressCallback = null,
            float progress = -1f,
            CancellationToken cancellationToken = default
        ) {
            return Play(
                data: progressCallback,
                progressCallback: (_, callback, p, oldP) => callback?.Invoke(p, oldP),
                progress,
                cancellationToken
            );
        }

        public void Stop() {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private async UniTask TrackProgress<T>(
            T data,
            float duration,
            float speed,
            ProgressCallback<TContext, T> progressCallback = null,
            CancellationToken cancellationToken = default
        ) {
            byte id = ++_trackProgressId;

            if (duration <= 0f) {
                float oldProgress = _progress;
                _progress = speed >= 0f ? 1f : 0f;
                
                if (!oldProgress.IsNearlyEqual(_progress)) {
                    progressCallback?.Invoke(Context, data, _progress, oldProgress);
                    OnProgressUpdate.Invoke(_progress, oldProgress);
                }
                
                return;
            }

#if UNITY_EDITOR
            float time = Time.realtimeSinceStartup;
#endif
            
            while (!cancellationToken.IsCancellationRequested && id == _trackProgressId) {
                float dt = Time.deltaTime;

#if UNITY_EDITOR
                if (!Application.isPlaying) {
                    dt = Time.realtimeSinceStartup - time;
                    time = Time.realtimeSinceStartup;
                }
#endif
                
                float oldProgress = _progress;
                _progress = Mathf.Clamp01(_progress + dt * speed / duration);

                if (!oldProgress.IsNearlyEqual(_progress)) {
                    progressCallback?.Invoke(Context, data, _progress, oldProgress);
                    OnProgressUpdate.Invoke(_progress, oldProgress);
                }

                if (speed > 0 && _progress >= 1f || speed < 0 && _progress <= 0f) {
                    break;
                }

                await UniTask.Yield();
            }
        }

        private void SetTween(TTween tween) {
            Stop();
            _tween = tween;
            _playState = PlayState.DurationNotSet;
        }

        private void SetProgress(float value) {
            Stop();
            _progress = Mathf.Clamp01(value);
            
#if UNITY_EDITOR
            _tween?.Play(context: default, Duration, _progress, speed: 0f).Forget();
#endif
        }

        private void SetSpeed(float value) {
            Stop();
            _speed = value;
        }

        private float GetDuration(bool forceRecalculate) {
            if (forceRecalculate || _playState == PlayState.DurationNotSet) {
                _tween?.CreateNextDuration();
                if (_playState == PlayState.DurationNotSet) _playState = PlayState.DurationCalculated;
            }
            
            return Mathf.Max(_tween?.Duration ?? 0f, 0f);
        }
    }

}
