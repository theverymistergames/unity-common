using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Tick;

namespace MisterGames.Common.Async {
    
    public static class AsyncExt {
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RecreateCts(ref CancellationTokenSource cts) {
            cts?.Cancel();
            cts?.Dispose();
            cts = new CancellationTokenSource();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeCts(ref CancellationTokenSource cts) {
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniTask WhenAll(UniTask t0, UniTask t1) {
            return UniTask.WhenAll(t0, t1);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask WhenSequence(UniTask t0, UniTask t1, CancellationToken cancellationToken = default) {
            await t0;
            if (cancellationToken.IsCancellationRequested) return;

            await t1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask WhenSequence(IReadOnlyList<UniTask> tasks, CancellationToken cancellationToken = default) {
            for (int i = 0; i < tasks?.Count && !cancellationToken.IsCancellationRequested; i++) {
                await tasks[i];
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask Delay(float delay, bool ignoreTimeScale, CancellationToken cancellationToken) {
            float t = 0f;
            float speed = delay > 0f ? 1f / delay : float.MaxValue;

            while (t < 1f && !cancellationToken.IsCancellationRequested) {
                float dt = ignoreTimeScale ? TimeSources.unscaledDeltaTime : TimeSources.deltaTime;
                t += dt * speed;
                await UniTask.Yield();
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask Delay(TimeSpan timeSpan, bool ignoreTimeScale, CancellationToken cancellationToken) {
            double t = 0f;
            double speed = timeSpan.TotalSeconds > 0f ? 1f / timeSpan.TotalSeconds : float.MaxValue;

            while (t < 1f && !cancellationToken.IsCancellationRequested) {
                float dt = ignoreTimeScale ? TimeSources.unscaledDeltaTime : TimeSources.deltaTime;
                t += dt * speed;
                await UniTask.Yield();
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask Delay(float delay, CancellationToken cancellationToken) {
            float t = 0f;
            float speed = delay > 0f ? 1f / delay : float.MaxValue;

            while (t < 1f && !cancellationToken.IsCancellationRequested) {
                t += TimeSources.deltaTime * speed;
                await UniTask.Yield();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask Delay(TimeSpan timeSpan, CancellationToken cancellationToken) {
            double t = 0f;
            double speed = timeSpan.TotalSeconds > 0f ? 1f / timeSpan.TotalSeconds : float.MaxValue;

            while (t < 1f && !cancellationToken.IsCancellationRequested) {
                t += TimeSources.deltaTime * speed;
                await UniTask.Yield();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask DelayUnscaled(float delay, CancellationToken cancellationToken) {
            float t = 0f;
            float speed = delay > 0f ? 1f / delay : float.MaxValue;

            while (t < 1f && !cancellationToken.IsCancellationRequested) {
                t += TimeSources.unscaledDeltaTime * speed;
                await UniTask.Yield();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask DelayUnscaled(TimeSpan timeSpan, CancellationToken cancellationToken) {
            double t = 0f;
            double speed = timeSpan.TotalSeconds > 0f ? 1f / timeSpan.TotalSeconds : float.MaxValue;

            while (t < 1f && !cancellationToken.IsCancellationRequested) {
                t += TimeSources.unscaledDeltaTime * speed;
                await UniTask.Yield();
            }
        }
    }
    
}