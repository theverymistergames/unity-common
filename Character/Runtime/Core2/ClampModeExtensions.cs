using System;
using UnityEngine;

namespace MisterGames.Character.Core2 {

    public static class ClampModeExtensions {

        public static float ApplyClamp(this float value, ClampMode mode, float a, float b) => mode switch {
            ClampMode.None => value,
            ClampMode.Lower => Mathf.Max(a, value),
            ClampMode.Upper => Mathf.Min(value, b),
            ClampMode.Both => Mathf.Min(Mathf.Max(a, value), b),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };

        public static int ApplyClamp(this int value, ClampMode mode, int a, int b) => mode switch {
            ClampMode.None => value,
            ClampMode.Lower => Math.Max(a, value),
            ClampMode.Upper => Math.Min(value, b),
            ClampMode.Both => Math.Min(Mathf.Max(a, value), b),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };

        public static Vector2 ApplyClamp(this Vector2 value, ClampMode xMode, ClampMode yMode, Vector2 a, Vector2 b) =>
            new Vector2(
                value.x.ApplyClamp(xMode, a.x, b.x),
                value.y.ApplyClamp(yMode, a.y, b.y)
            );

        public static Vector3 ApplyClamp(this Vector3 value, ClampMode xMode, ClampMode yMode, ClampMode zMode, Vector3 a, Vector3 b) =>
            new Vector3(
                value.x.ApplyClamp(xMode, a.x, b.x),
                value.y.ApplyClamp(yMode, a.y, b.y),
                value.z.ApplyClamp(zMode, a.z, b.z)
            );

        public static Quaternion ApplyClamp(this Quaternion value, ClampMode xMode, ClampMode yMode, ClampMode zMode, Vector3 aEuler, Vector3 bEuler) =>
            Quaternion.Euler(
                value.x.ApplyClamp(xMode, aEuler.x, bEuler.x),
                value.y.ApplyClamp(yMode, aEuler.y, bEuler.y),
                value.z.ApplyClamp(zMode, aEuler.z, bEuler.z)
            );
    }

}
