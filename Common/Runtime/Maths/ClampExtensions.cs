using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MisterGames.Common.Maths {

    public static class ClampExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(this float value, ClampMode mode, float a, float b) => mode switch {
            ClampMode.None => value,
            ClampMode.Lower => Mathf.Max(a, value),
            ClampMode.Upper => Mathf.Min(value, b),
            ClampMode.Full => Mathf.Clamp(value, a, b),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(this int value, ClampMode mode, int a, int b) => mode switch {
            ClampMode.None => value,
            ClampMode.Lower => Math.Max(a, value),
            ClampMode.Upper => Math.Min(value, b),
            ClampMode.Full => Math.Clamp(value, a, b),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

}
