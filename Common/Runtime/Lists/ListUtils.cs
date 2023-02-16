using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace MisterGames.Common.Lists {

    public static class ListUtils {

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

        public static T[] Slice<T>(this T[] array, int start, int end) {
            int length = end - start + 1;
            var result = new T[length];
            for (int i = start; i <= end; i++) {
                result[i] = array[i];
            }
            return result;
        }

        public static T GetRandom<T>(this IReadOnlyList<T> list) {
            if (list.Count == 0) return default;
            int index = Random.Range(0, list.Count - 1);
            return list[index];
        }
        
    }

}
