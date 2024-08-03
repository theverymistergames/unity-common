using System;
using System.Collections.Generic;

namespace MisterGames.Common.Lists {

    public static class ArrayExtensions {
        public static bool TryFind<T>(this IReadOnlyList<T> list, Func<T, bool> predicate, out T value) {
            int count = list?.Count ?? 0;
            
            for (int i = 0; i < count; i++) {
                var t = list![i];
                if (!predicate.Invoke(t)) continue;
                
                value = t;
                return true;
            }

            value = default;
            return false;
        }
        
        public static bool TryFind<T, S>(this IReadOnlyList<T> list, S data, Func<T, S, bool> predicate, out T value) {
            int count = list?.Count ?? 0;
            
            for (int i = 0; i < count; i++) {
                var t = list![i];
                if (!predicate.Invoke(t, data)) continue;
                
                value = t;
                return true;
            }

            value = default;
            return false;
        }
        
        public static T FirstOrDefault<T>(this IReadOnlyList<T> list, Func<T, bool> predicate, T defaultValue = default) {
            return TryFind(list, predicate, out var value) ? value : defaultValue;
        }
        
        public static T FirstOrDefault<T, S>(this IReadOnlyList<T> list, S data, Func<T, S, bool> predicate, T defaultValue = default) {
            return TryFind(list, data, predicate, out var value) ? value : defaultValue;
        }
        
        public static bool Contains<T>(this IReadOnlyList<T> list, T value) {
            for (int i = 0; i < list.Count; i++) {
                if (EqualityComparer<T>.Default.Equals(list[i], value)) return true;
            }

            return false;
        }

        public static T GetRandom<T>(this IReadOnlyList<T> list) {
            return list.Count == 0 ? default : list[UnityEngine.Random.Range(0, list.Count)];
        }
        
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
