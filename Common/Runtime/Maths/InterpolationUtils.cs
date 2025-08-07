using System.Runtime.CompilerServices;

namespace MisterGames.Common.Maths {

    public static class InterpolationUtils {

        /// <summary>
        /// Edge interpolation is a value in range [0f, 1f], the result of the function y = g(t, w), where:
        /// - y is the result value;
        /// - t is the input interpolation, a value in range [0f, 1f];
        /// - w is the edge weight, a value in range [0f, 1f].
        ///
        /// The result depends on t:
        /// 1) t = [0f, w]: result is linearly interpolated from 0f to 1f (y = t / w);
        /// 2) t = (w, 1f - w): result is 1f;
        /// 3) t = [1f - w, 1f]: result is linearly interpolated from 1f to 0f (y = (1f - t) / w).
        ///
        /// The value of edge interpolation can be used to make smooth transitions between two axes around their edges.
        /// </summary>
        public static float GetEdgeInterpolation(float edgeWeight, float t) {
            // t is outside of range (0f, 1f): keep a value, if value = lerp(a, b, t)
            if (t is <= 0f or >= 1f) return 0f;

            // w is 0f: keep b value, if value = lerp(a, b, t)
            if (edgeWeight <= 0f) return 1f;

            // w is 1f: keep a value, if value = lerp(a, b, t)
            if (edgeWeight >= 1f) return 0f;

            if (t <= 0.5f) {
                // t = [0f, w), but while t <= 0.5f
                if (t < edgeWeight) return t / edgeWeight;

                // t = [w, 0.5f]: keep b value, if value = lerp(a, b, t)
                return 1f;
            }

            // t = (0.5f, 1f - w]: keep b value, if value = lerp(a, b, t)
            if (t <= 1f - edgeWeight) return 1f;

            // t = (1f - w, 1f), but while t > 0.5f
            return (1f - t) / edgeWeight;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Remap01(float a, float b, float t) {
            return b - a > 0f 
                ? (t - a) / (b - a)
                : t > a ? 1f : 0f;
        }
    }

}
