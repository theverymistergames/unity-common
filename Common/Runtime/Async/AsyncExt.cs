using System;
using System.Buffers;
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
        public static async UniTask WhenAll(UniTask t0, UniTask t1) {
            var tasks = ArrayPool<UniTask>.Shared.Rent(2);
            
            tasks[0] = t0;
            tasks[1] = t1;
            
            await UniTask.WhenAll(tasks);
            
            ArrayPool<UniTask>.Shared.Return(tasks, clearArray: true);
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
        public static async UniTask Delay(float delay, bool ignoreTimescale, CancellationToken cancellationToken) {
            float t = 0f;
            float speed = delay > 0f ? 1f / delay : float.MaxValue;

            while (t < 1f && !cancellationToken.IsCancellationRequested) {
                float dt = ignoreTimescale ? TimeSources.unscaledDeltaTime : TimeSources.deltaTime;
                t += dt * speed;
                await UniTask.Yield();
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask Delay(TimeSpan timeSpan, bool ignoreTimescale, CancellationToken cancellationToken) {
            double t = 0f;
            double speed = timeSpan.TotalSeconds > 0f ? 1f / timeSpan.TotalSeconds : float.MaxValue;

            while (t < 1f && !cancellationToken.IsCancellationRequested) {
                float dt = ignoreTimescale ? TimeSources.unscaledDeltaTime : TimeSources.deltaTime;
                t += dt * speed;
                await UniTask.Yield();
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask DelayScaled(float delay, CancellationToken cancellationToken) {
            float t = 0f;
            float speed = delay > 0f ? 1f / delay : float.MaxValue;

            while (t < 1f && !cancellationToken.IsCancellationRequested) {
                t += TimeSources.deltaTime * speed;
                await UniTask.Yield();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask DelayScaled(TimeSpan timeSpan, CancellationToken cancellationToken) {
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