using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;

namespace MisterGames.Common.Jobs {
    
    public static class JobExt {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BatchFor(int count) {
            return math.max(count / JobsUtility.JobWorkerCount, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref T GetRef<T>(this NativeArray<T> array, int index) where T : unmanaged {
            var ptr = (T*) array.GetUnsafePtr();
            return ref ptr[index];
        }
    }
    
}