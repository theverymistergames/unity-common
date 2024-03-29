﻿using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Common.Lists {

    public static class ListExtensions {

        public static bool Contains<T>(this IReadOnlyList<T> list, T value) {
            for (int i = 0; i < list.Count; i++) {
                if (EqualityComparer<T>.Default.Equals(list[i], value)) return true;
            }

            return false;
        }

        public static T GetRandom<T>(this IReadOnlyList<T> list) {
            return list.Count == 0 ? default : list[Random.Range(0, list.Count)];
        }
    }

}
