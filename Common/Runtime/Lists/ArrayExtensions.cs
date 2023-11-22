using System;
using System.Runtime.CompilerServices;

namespace MisterGames.Common.Lists {

    public static class ArrayExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureCapacity<T>(ref T[] array, int min, int max = -1) {
            if (min < 0) min = 0;

            if (array == null) {
                array = min == 0 ? Array.Empty<T>() : new T[min];
                return;
            }

            int current = array.Length;
            if (current >= min && current <= max) return;

            int newCapacity = current * 2;
            if (newCapacity < min) newCapacity = min;
            if (max >= 0 && newCapacity > max) newCapacity = max;

            Array.Resize(ref array, newCapacity);
        }
    }

}
