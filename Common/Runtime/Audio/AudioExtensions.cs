using System.Runtime.CompilerServices;
using Unity.Burst;
using UnityEngine;

namespace MisterGames.Common.Audio {
    
    public static class AudioExtensions {

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InMask(this AudioParameter parameter, int mask) {
            return (mask & 1 << (int) parameter) > 0;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteToMask(this AudioParameter parameter, ref int mask) {
            mask |= 1 << (int) parameter;
        }
        
        public static int GetRandomIndex(ref int indicesMask, ref int startIndex, int lastIndex, int count) {
            switch (count) {
                case 1:
                    return 0;
                
                case 2:
                case 3:
                    return Random.Range(0, count);
            }
            
            const int bits = 32;
            
            int max = Mathf.Min(bits, count - startIndex);
            int freeCount = max;
            int r;
            
            for (int i = 0; i < max; i++) {
                if ((indicesMask & (1 << i)) != 0) freeCount--;
            }

            if (freeCount <= 0) {
                startIndex += bits;
                if (startIndex > count - 1) startIndex = 0;

                if (count > bits) {
                    r = Random.Range(0, Mathf.Min(bits, count - startIndex));
                    indicesMask = 1 << r;
                    return r + startIndex;
                }
                
                indicesMask = 0;
                max = Mathf.Min(bits, count - startIndex);
                freeCount = max - 1;
            }
            
            r = Random.Range(0, freeCount);
            
            if (freeCount >= max) {
                indicesMask |= 1 << r;
                return r + startIndex;
            }
            
            freeCount = 0;
            for (int i = 0; i < max; i++) {
                if ((indicesMask & (1 << i)) != 0 || i + startIndex == lastIndex || freeCount++ != r) {
                    continue;
                }
                
                indicesMask |= 1 << i;
                return i + startIndex;
            }

            return Random.Range(0, count);
        }
    }
    
}