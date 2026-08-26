using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;

namespace MisterGames.Common.Jobs {
    
    public static class JobExt {

        /// <summary>
        /// Batch size for a parallel job over <paramref name="count"/> items.
        /// <paramref name="minBatch"/> prevents one-item batches for cheap kernels,
        /// where per-batch overhead is bigger than the work itself.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BatchFor(int count, int minBatch = 1) {
            return math.max(count / JobsUtility.JobWorkerCount, minBatch);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref T GetRef<T>(this NativeArray<T> array, int index) where T : unmanaged {
            var ptr = (T*) array.GetUnsafePtr();
            return ref ptr[index];
        }
    }
    
}
