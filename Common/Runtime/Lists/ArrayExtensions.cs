using System;
using System.Runtime.CompilerServices;

namespace MisterGames.Common.Lists {

    public static class ArrayExtensions {

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

        public static void ResetArrayElements<T>(this T[] array, int count = -1) {
            if (count < 0) count = array.Length;

            for (int i = 0; i < count; i++) {
                array[i] = default;
            }
        }
    }

}
