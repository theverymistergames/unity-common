using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using Random = UnityEngine.Random;

namespace MisterGames.Common.Lists {

    public static class ArrayExtensions {

        public static void Shuffle<T>(this T[] array, int length = -1) {
            int n = length < 0 ? array.Length : length;
            while (n > 1) {
                int k = Random.Range(0, n--);
                (array[n], array[k]) = (array[k], array[n]);
            }
        }
        
        public static void Shuffle<T>(this IList<T> array, int length = -1) {
            int n = length < 0 ? array.Count : length;
            while (n > 1) {
                int k = Random.Range(0, n--);
                (array[n], array[k]) = (array[k], array[n]);
            }
        }

        /// <summary>
        /// Place valid elements at start. 
        /// </summary>
        public static void RemoveIf<T>(this T[] list, ref int count, Func<T, bool> predicate) {
            for (int i = count - 1; i >= 0; i--) {
                var t = list[i];
                if (!predicate.Invoke(t)) continue;

                int lastValid = --count;
                list[i] = list[lastValid];
                list[lastValid] = t;
            }
        }
        
        /// <summary>
        /// Remove elements with positive predicate.
        /// </summary>
        public static void RemoveIf<T>(this List<T> list, Func<T, bool> predicate) {
            int count = list.Count;
            
            for (int i = count - 1; i >= 0; i--) {
                var t = list[i];
                if (!predicate.Invoke(t)) continue;
                
                int lastValid = --count;
                list[i] = list[lastValid];
                list[lastValid] = t;
            }
            
            list.RemoveRange(count, list.Count - count);
        }
        
        public static float WriteToCircularBufferAndGetAverage(this float[] buffer, float value, ref int pointer) {
            buffer[pointer++ % buffer.Length] = value;
            
            float sum = 0f;
            int count = Math.Min(pointer, buffer.Length);
            
            for (int i = 0; i < count; i++) {
                sum += buffer[i];
            }
            
            return sum / count;
        }

        public static Vector2 WriteToCircularBufferAndGetAverage(this Vector2[] buffer, Vector2 value, ref int pointer) {
            buffer[pointer++ % buffer.Length] = value;
            
            Vector2 sum = default;
            int count = Math.Min(pointer, buffer.Length);
            
            for (int i = 0; i < count; i++) {
                sum += buffer[i];
            }
            
            return sum / count;
        }

        public static Vector3 WriteToCircularBufferAndGetAverage(this Vector3[] buffer, Vector3 value, ref int pointer) {
            buffer[pointer++ % buffer.Length] = value;
            
            Vector3 sum = default;
            int count = Math.Min(pointer, buffer.Length);
            
            for (int i = 0; i < count; i++) {
                sum += buffer[i];
            }
            
            return sum / count;
        }

        public static bool Any<T>(this ReadOnlyArray<T> list, Func<T, bool> predicate) {
            return TryFind(list, predicate, out _);
        }
        
        public static bool Any<T>(this IReadOnlyList<T> list, Func<T, bool> predicate) {
            return TryFind(list, predicate, out _);
        }
        
        public static bool Any<T, S>(this ReadOnlyArray<T> list, S data, Func<T, S, bool> predicate) {
            return TryFind(list, data, predicate, out _);
        }
        
        public static bool Any<T, S>(this IReadOnlyList<T> list, S data, Func<T, S, bool> predicate) {
            return TryFind(list, data, predicate, out _);
        }
        
        public static bool Contains<T>(this IReadOnlyList<T> list, T element) {
            int count = list.Count;
            for (int i = 0; i < count; i++) {
                if (EqualityComparer<T>.Default.Equals(list![i], element)) return true;
            }

            return false;
        }
        
        public static bool TryFind<T>(this ReadOnlyArray<T> list, Func<T, bool> predicate, out T value) {
            int count = list.Count;
            for (int i = 0; i < count; i++) {
                var t = list[i];
                if (!predicate.Invoke(t)) continue;
                
                value = t;
                return true;
            }

            value = default;
            return false;
        }
        
        public static bool TryFind<T, S>(this ReadOnlyArray<T> list, S data, Func<T, S, bool> predicate, out T value) {
            int count = list.Count;
            for (int i = 0; i < count; i++) {
                var t = list[i];
                if (!predicate.Invoke(t, data)) continue;
                
                value = t;
                return true;
            }

            value = default;
            return false;
        }
        
        public static bool TryFind<T>(this IReadOnlyList<T> list, Func<T, bool> predicate, out T value) {
            int count = list.Count;
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
            int count = list.Count;
            for (int i = 0; i < count; i++) {
                var t = list![i];
                if (!predicate.Invoke(t, data)) continue;
                
                value = t;
                return true;
            }

            value = default;
            return false;
        }
        
        public static int TryFindIndex<T>(this ReadOnlyArray<T> list, Func<T, bool> predicate) {
            int count = list.Count;
            for (int i = 0; i < count; i++) {
                if (predicate.Invoke(list[i])) return i;
            }
            
            return -1;
        }
        
        public static int TryFindIndex<T, S>(this ReadOnlyArray<T> list, S data, Func<T, S, bool> predicate) {
            int count = list.Count;
            for (int i = 0; i < count; i++) {
                if (predicate.Invoke(list[i], data)) return i;
            }
            
            return -1;
        }
        
        public static int TryFindIndex<T>(this IReadOnlyList<T> list, Func<T, bool> predicate) {
            int count = list.Count;
            for (int i = 0; i < count; i++) {
                if (predicate.Invoke(list![i])) return i;
            }

            return -1;
        }
        
        public static int TryFindIndex<T, S>(this IReadOnlyList<T> list, S data, Func<T, S, bool> predicate) {
            int count = list.Count;
            for (int i = 0; i < count; i++) {
                if (predicate.Invoke(list![i], data)) return i;
            }

            return -1;
        }
        
        public static T FirstOrDefault<T>(this IReadOnlyList<T> list, Func<T, bool> predicate, T defaultValue = default) {
            return TryFind(list, predicate, out var value) ? value : defaultValue;
        }
        
        public static T FirstOrDefault<T, S>(this IReadOnlyList<T> list, S data, Func<T, S, bool> predicate, T defaultValue = default) {
            return TryFind(list, data, predicate, out var value) ? value : defaultValue;
        }

        public static T GetRandom<T>(this IReadOnlyList<T> list, int tryExcludeIndex = -1) {
            int count = list?.Count ?? 0;
            return count == 0 ? default : list![GetRandom(0, count, tryExcludeIndex)];
        }
        
        public static int GetRandomIndex<T>(this IReadOnlyList<T> list, int tryExcludeIndex = -1) {
            return GetRandom(0, list?.Count ?? 0, tryExcludeIndex);
        }

        public static int GetRandom(int minInclusive, int maxExclusive, int tryExclude) {
            int r = Random.Range(minInclusive, maxExclusive);
            
            if (r != tryExclude || Mathf.Abs(minInclusive - maxExclusive) <= 1) {
                return r;
            }

            if (minInclusive > maxExclusive) {
                (minInclusive, maxExclusive) = (maxExclusive, minInclusive);
            }
            
            if (maxExclusive - tryExclude <= 1) {
                return Random.Range(minInclusive, tryExclude);
            }

            if (tryExclude - minInclusive <= 0) {
                return Random.Range(tryExclude + 1, maxExclusive);
            }

            return Random.value < 0.5f ? Random.Range(minInclusive, tryExclude) : Random.Range(tryExclude + 1, maxExclusive);
        }
        
        public static void EnsureCapacity<T>(ref T[] array, int min, int max = -1) {
            if (min < 0) min = 0;

            if (array == null) {
                array = min == 0 ? Array.Empty<T>() : new T[min];
                return;
            }

            int current = array.Length;
            if (current >= min && (max < 0 || current <= max)) return;

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
