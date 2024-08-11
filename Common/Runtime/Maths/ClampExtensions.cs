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
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Clamp(this Vector2 vector, Vector2 min, Vector2 max)
        {
            return new Vector2(
                Mathf.Clamp(vector.x, min.x, max.x),
                Mathf.Clamp(vector.y, min.y, max.y)
            );
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Clamp(this Vector3 vector, Vector3 min, Vector3 max) {
            return new Vector3(
                Mathf.Clamp(vector.x, min.x, max.x), 
                Mathf.Clamp(vector.y, min.y, max.y),
                Mathf.Clamp(vector.z, min.z, max.z)
            );
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ClampAngle(this float value, ClampMode mode, float a, float b) => mode switch {
            ClampMode.None => value.ClampAngle(-180f, 180f),
            ClampMode.Lower => value.ClampAngle(a, 180f),
            ClampMode.Upper => value.ClampAngle(-180f, b),
            ClampMode.Full => value.ClampAngle(a, b),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ClampAngle(this float value, float min, float max) {
            float dtAngle = Mathf.Abs((min - max + 180f) % 360f - 180f);
            float hdtAngle = dtAngle * 0.5f;
            float midAngle = min + hdtAngle;
            
            float offset = Mathf.Abs(Mathf.DeltaAngle(value, midAngle)) - hdtAngle;
            if (offset > 0) value = Mathf.MoveTowardsAngle(value, midAngle, offset);
           
            return value;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ClampAngle(this Vector2 value, Vector2 min, Vector2 max) {
            return new Vector2(
                ClampAngle(value.x, min.x, max.x),
                ClampAngle(value.y, min.y, max.y)
            );
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ClampAngle(this Vector3 value, Vector3 min, Vector3 max) {
            return new Vector3(
                ClampAngle(value.x, min.x, max.x),
                ClampAngle(value.y, min.y, max.y),
                ClampAngle(value.z, min.z, max.z)
            );
        }
    }

}
