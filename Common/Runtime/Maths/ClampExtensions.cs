using System;
using UnityEngine;

namespace MisterGames.Character.Core2 {

    public static class ClampExtensions {

        public static float Clamp(this float value, ClampMode mode, float a, float b) => mode switch {
            ClampMode.None => value,
            ClampMode.Lower => Mathf.Max(a, value),
            ClampMode.Upper => Mathf.Min(value, b),
            ClampMode.Full => Mathf.Clamp(value, a, b),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };

        public static int Clamp(this int value, ClampMode mode, int a, int b) => mode switch {
            ClampMode.None => value,
            ClampMode.Lower => Math.Max(a, value),
            ClampMode.Upper => Math.Min(value, b),
            ClampMode.Full => Math.Clamp(value, a, b),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };

        public static Vector2 Clamp(this Vector2 value, ClampMode xMode, ClampMode yMode, Vector2 a, Vector2 b) =>
            new Vector2(
                value.x.Clamp(xMode, a.x, b.x),
                value.y.Clamp(yMode, a.y, b.y)
            );

        public static Vector3 Clamp(this Vector3 value, ClampMode xMode, ClampMode yMode, ClampMode zMode, Vector3 a, Vector3 b) =>
            new Vector3(
                value.x.Clamp(xMode, a.x, b.x),
                value.y.Clamp(yMode, a.y, b.y),
                value.z.Clamp(zMode, a.z, b.z)
            );

        public static Quaternion Clamp(this Quaternion value, ClampMode xMode, ClampMode yMode, ClampMode zMode, Vector3 aEuler, Vector3 bEuler) =>
            Quaternion.Euler(
                value.x.Clamp(xMode, aEuler.x, bEuler.x),
                value.y.Clamp(yMode, aEuler.y, bEuler.y),
                value.z.Clamp(zMode, aEuler.z, bEuler.z)
            );
    }

}
