using UnityEngine;

namespace MisterGames.Common.Maths {

    public static class NumberUtils {

        public static bool IsNearlyZero(this float value) {
            return Mathf.Abs(value) < Mathf.Epsilon;
        }
        
        public static bool IsNearlyEqual(this float value, float other) {
            return IsNearlyZero(value - other);
        }

        public static int AsInt(this bool value) {
            return value ? 1 : 0;
        }

        public static float AsFloat(this bool value) {
            return value ? 1f : 0f;
        }
    }

}
