using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Tweens.Core2 {

    public delegate void ProgressCallback(float progress);
    public delegate void ProgressCallback<in T>(T data, float progress);

    public static class TweenExtensions {

        public static void PlayAndForget(
            float duration,
            ProgressCallback progressCallback = null,
            float startProgress = 0f,
            float speed = 1f,
            PlayerLoopStage playerLoopStage = PlayerLoopStage.Update,
            CancellationToken cancellationToken = default
        ) {
            Play(duration, progressCallback, startProgress, speed, playerLoopStage, cancellationToken).Forget();
        }

        public static void PlayAndForget<T>(
            T data,
            float duration,
            ProgressCallback<T> progressCallback = null,
            float startProgress = 0f,
            float speed = 1f,
            PlayerLoopStage playerLoopStage = PlayerLoopStage.Update,
            CancellationToken cancellationToken = default
        ) {
            Play(data, duration, progressCallback, startProgress, speed, playerLoopStage, cancellationToken).Forget();
        }

        public static async UniTask Play(
            float duration,
            ProgressCallback progressCallback = null,
            float startProgress = 0f,
            float speed = 1f,
            PlayerLoopStage playerLoopStage = PlayerLoopStage.Update,
            CancellationToken cancellationToken = default
        ) {
            if (cancellationToken.IsCancellationRequested) return;

            float progress = Mathf.Clamp01(startProgress);

            if (duration <= 0f) {
                float oldProgress = speed > 0f ? 0f : 1f;
                progress = speed > 0f ? 1f : 0f;
                if (!oldProgress.IsNearlyEqual(progress)) progressCallback?.Invoke(progress);
                return;
            }

            var timeSource = TimeSources.Get(playerLoopStage);

            while (!cancellationToken.IsCancellationRequested) {
                float oldProgress = progress;
                progress = Mathf.Clamp01(progress + timeSource.DeltaTime * speed / duration);
                if (!oldProgress.IsNearlyEqual(progress)) progressCallback?.Invoke(progress);

                if (speed > 0f && progress >= 1f || speed < 0f && progress <= 0f) {
                    break;
                }

                await UniTask.Yield();
            }
        }

        public static async UniTask Play<T>(
            T data,
            float duration,
            ProgressCallback<T> progressCallback = null,
            float startProgress = 0f,
            float speed = 1f,
            PlayerLoopStage playerLoopStage = PlayerLoopStage.Update,
            CancellationToken cancellationToken = default
        ) {
            if (cancellationToken.IsCancellationRequested) return;

            float progress = Mathf.Clamp01(startProgress);

            if (duration <= 0f) {
                float oldProgress = speed > 0f ? 0f : 1f;
                progress = speed > 0f ? 1f : 0f;
                if (!oldProgress.IsNearlyEqual(progress)) progressCallback?.Invoke(data, progress);
                return;
            }

            var timeSource = TimeSources.Get(playerLoopStage);

            while (!cancellationToken.IsCancellationRequested) {
                float oldProgress = progress;
                progress = Mathf.Clamp01(progress + timeSource.DeltaTime * speed / duration);
                if (!oldProgress.IsNearlyEqual(progress)) progressCallback?.Invoke(data, progress);

                if (speed > 0f && progress >= 1f || speed < 0f && progress <= 0f) {
                    break;
                }

                await UniTask.Yield();
            }
        }
    }

}
