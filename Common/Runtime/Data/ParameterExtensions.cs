using System.Runtime.CompilerServices;
using UnityEngine;

namespace MisterGames.Common.Data {

    public static class ParameterExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CreateMultiplier(this FloatParameter p) {
            return p.multiplier + Random.Range(-p.addRandom, p.addRandom);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 CreateMultiplier(this Vector2Parameter p) {
            return new Vector2(
                p.x.CreateMultiplier(),
                p.y.CreateMultiplier()
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 CreateMultiplier(this Vector3Parameter p) {
            return new Vector3(
                p.x.CreateMultiplier(),
                p.y.CreateMultiplier(),
                p.z.CreateMultiplier()
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Evaluate(this FloatParameter p, float t) {
            return p.curve.Evaluate(t);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Evaluate(this Vector2Parameter p, float t) {
            return new Vector2(
                p.x.Evaluate(t),
                p.y.Evaluate(t)
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Evaluate(this Vector3Parameter p, float t) {
            return new Vector3(
                p.x.Evaluate(t),
                p.y.Evaluate(t),
                p.z.Evaluate(t)
            );
        }
    }

}
