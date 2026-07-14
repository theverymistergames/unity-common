using System.Buffers;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;

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
        public static async UniTask DelayTimescaled(float delay, CancellationToken cancellationToken) {
            float t = 0f;
            float speed = delay > 0f ? 1f / delay : float.MaxValue;

            while (t < 1f && !cancellationToken.IsCancellationRequested) {
                t += UnityEngine.Time.deltaTime * speed;
                await UniTask.Yield();
            }
        }

    }
    
}