using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.Common.Lists {

    public static class ArrayExtensions {

        public static void Shuffle<T>(this T[] array, int length = -1) {
            int n = length < 0 ? array.Length : length;
            while (n > 1) 
            {
                int k = Random.Range(0, n--);
                (array[n], array[k]) = (array[k], array[n]);
            }
        }
        
        public static void Shuffle<T>(this IList<T> array, int length = -1) {
            int n = length < 0 ? array.Count : length;
            while (n > 1) 
            {
                int k = Random.Range(0, n--);
                (array[n], array[k]) = (array[k], array[n]);
            }
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
            return list.Count == 0 ? default : list[Random.Range(0, list.Count)];
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
