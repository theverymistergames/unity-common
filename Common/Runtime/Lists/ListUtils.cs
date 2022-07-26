﻿using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace MisterGames.Common.Lists {

    public static class ListUtils {

        public static bool Some<T>(this IReadOnlyList<T> list, Func<T, bool> predicate) {
            for (int i = 0; i < list.Count; i++) {
                if (predicate.Invoke(list[i])) {
                    return true;
                }
            }

            return false;
        }
        
        public static bool Contains<T>(this IReadOnlyList<T> list, T value) {
            for (int i = 0; i < list.Count; i++) {
                if (Equals(list[i], value)) return true;
            }

            return false;
        }

        public static bool IsEmpty<T>(this IEnumerable<T> enumerable) {
            return !enumerable.Any();
        }

        public static List<T> Reversed<T>(this List<T> list) {
            list.Reverse();
            return list;
        }

        public static IEnumerable<T> RemoveIf<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate) {
            return enumerable.Where(e => !predicate.Invoke(e));
        }
        
        public static IEnumerable<T> DistinctBy<T, P>(this IEnumerable<T> enumerable, Func<T, P> selector) {
            return enumerable.GroupBy(selector).Select(groups => groups.First());
        }

        public static void AddTo<T>(this IEnumerable<T> enumerable, List<T> other) {
            other.AddRange(enumerable);
        }

        public static T[] Slice<T>(this T[] array, int start, int end) {
            int length = end - start + 1;
            var result = new T[length];
            for (int i = start; i <= end; i++) {
                result[i] = array[i];
            }
            return result;
        }
        
        public static List<T> Plus<T>(this IEnumerable<T> list, IEnumerable<T> other) {
            var result = new List<T>(list);
            result.AddRange(other);
            return result;
        }
        
        public static List<T> Plus<T>(this List<T> list, IEnumerable<T> other) {
            var result = new List<T>(list);
            result.AddRange(other);
            return result;
        }
        
        public static List<T> Plus<T>(this IEnumerable<T> list, T element) {
            return new List<T>(list) { element };
        }
        
        public static List<T> Plus<T>(this List<T> list, T element) {
            return new List<T>(list) { element };
        }

        public static T GetRandom<T>(this IReadOnlyList<T> list) {
            if (list.Count == 0) return default;
            int index = Random.Range(0, list.Count - 1);
            return list[index];
        }
        
    }

}
