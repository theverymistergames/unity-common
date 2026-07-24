using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace MisterGames.Common.Maths {

    public static class NumberExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Abs(this float value) {
            return math.abs(value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Abs(this int value) {
            return math.abs(value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNearlyZero(this float value) {
            return math.abs(value) <= math.EPSILON;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNearlyZero(this float value, float tolerance) {
            return math.abs(value) <= tolerance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNearlyEqual(this float value, float other) {
            return math.abs(value - other) <= math.EPSILON;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNearlyEqual(this float value, float other, float tolerance) {
            return math.abs(value - other) <= tolerance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AsInt(this bool value) {
            return value ? 1 : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AsFloat(this bool value) {
            return value ? 1f : 0f;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LongAsTwoInts(long l, out int a, out int b) {
            a = (int) (l & uint.MaxValue);
            b = (int) (l >> 32);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UlongAsTwoInts(ulong l, out int a, out int b) {
            a = (int) (l & uint.MaxValue);
            b = (int) (l >> 32);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long TwoIntsAsLong(int a, int b) {
            long l = b;
            l <<= 32;
            l |= (uint) a;
            return l;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong TwoIntsAsUlong(int a, int b) {
            ulong l = (uint) b;
            l <<= 32;
            l |= (uint) a;
            return l;
        }

        /// <summary>
        /// Returns sine of 2 * pi * t multiplied by frequency that is linearly interpolated from f0 to f1 depending on t.
        /// If abs of result is less than threshold thr, result sign is used instead.
        /// </summary>
        public static float Oscillate(float t, float f0, float f1, float thr, float phase = 0f) {
            float f = f0 + (f1 - f0) * t;
            float v = math.sin(2f * math.PI * (t * f + phase));
            return math.abs(v) < thr ? v : math.sign(v);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InRange(this float value, Vector2 range) {
            return value >= range.x && value <= range.y;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Map01(this float value, Vector2 range) {
            return range.x.IsNearlyEqual(range.y)
                ? range.x
                : (value - range.x) / (range.y - range.x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte IncrementUncheckedRef(this ref byte value) {
            unchecked {
                return ++value;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IncrementUncheckedRef(this ref int value) {
            unchecked {
                return ++value;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ReturnThenIncrementUncheckedRef(this ref byte value) {
            unchecked {
                return value++;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReturnThenIncrementUncheckedRef(this ref int value) {
            unchecked {
                return value++;
            }
        }
    }

}
