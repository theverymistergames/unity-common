using UnityEngine;

namespace MisterGames.Common.Maths {

    public static class NumberExtensions {

        public static readonly float SqrEpsilon = Mathf.Epsilon * Mathf.Epsilon;

        public static bool IsNearlyZero(this float value) {
            return Mathf.Abs(value) < Mathf.Epsilon;
        }

        public static bool IsNearlyZero(this float value, float tolerance) {
            return Mathf.Abs(value) <= tolerance;
        }

        public static bool IsNearlyEqual(this float value, float other) {
            return IsNearlyZero(value - other);
        }

        public static bool IsNearlyEqual(this float value, float other, float tolerance) {
            return IsNearlyZero(value - other, tolerance);
        }

        public static int AsInt(this bool value) {
            return value ? 1 : 0;
        }

        public static float AsFloat(this bool value) {
            return value ? 1f : 0f;
        }
    }

}
