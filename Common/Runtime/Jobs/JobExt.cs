using System.Runtime.CompilerServices;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;

namespace MisterGames.Common.Jobs {
    
    public static class JobExt {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BatchFor(int count) {
            return math.max(count / JobsUtility.JobWorkerCount, 1);
        }
        
    }
    
}