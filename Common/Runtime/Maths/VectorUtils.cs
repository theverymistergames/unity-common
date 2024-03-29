﻿using System.Runtime.CompilerServices;
using UnityEngine;

namespace MisterGames.Common.Maths {

    public static class VectorUtils {

        // ---------------- ---------------- Equality ---------------- ----------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNearlyZero(this Vector3 vector) {
            return vector.x.IsNearlyZero() && vector.y.IsNearlyZero() && vector.z.IsNearlyZero();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNearlyZero(this Vector3 vector, float tolerance) {
            return vector.x.IsNearlyZero(tolerance) && vector.y.IsNearlyZero(tolerance) && vector.z.IsNearlyZero(tolerance);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNearlyZero(this Vector2 vector) {
            return vector.x.IsNearlyZero() && vector.y.IsNearlyZero();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNearlyZero(this Vector2 vector, float tolerance) {
            return vector.x.IsNearlyZero(tolerance) && vector.y.IsNearlyZero(tolerance);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNearlyEqual(this Vector3 vector, Vector3 other) {
            return vector.x.IsNearlyEqual(other.x) && vector.y.IsNearlyEqual(other.y) && vector.z.IsNearlyEqual(other.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNearlyEqual(this Vector3 vector, Vector3 other, float tolerance) {
            return vector.x.IsNearlyEqual(other.x, tolerance) && vector.y.IsNearlyEqual(other.y, tolerance) && vector.z.IsNearlyEqual(other.z, tolerance);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNearlyEqual(this Vector2 vector, Vector2 other) {
            return vector.x.IsNearlyEqual(other.x) && vector.y.IsNearlyEqual(other.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNearlyEqual(this Vector2 vector, Vector2 other, float tolerance) {
            return vector.x.IsNearlyEqual(other.x, tolerance) && vector.y.IsNearlyEqual(other.y, tolerance);
        }

        // ---------------- ---------------- Modification ---------------- ----------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Inverted(this Vector3 vector) {
            return -1f * vector;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Inverted(this Vector2 vector) {
            return -1f * vector;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 WithX(this Vector3 vector, float x) {
            return new Vector3(x, vector.y, vector.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 WithY(this Vector3 vector, float y) {
            return new Vector3(vector.x, y, vector.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 WithZ(this Vector3 vector, float z) {
            return new Vector3(vector.x, vector.y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 WithX(this Vector2 vector, float x) {
            return new Vector2(x, vector.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 WithY(this Vector2 vector, float y) {
            return new Vector2(vector.x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Multiply(this Vector2 a, Vector2 b) {
            return new Vector2(a.x * b.x, a.y * b.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Multiply(this Vector2 a, float x, float y) {
            return new Vector2(a.x * x, a.y * y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Multiply(this Vector3 a, float x, float y, float z) {
            return new Vector3(a.x * x, a.y * y, a.z * z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Multiply(this Vector3 a, Vector3 b) {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        // ---------------- ---------------- Geometry ---------------- ----------------

        public static Vector3 FindNearestPointOnSegment(Vector3 start, Vector3 end, Vector3 point) {
            var direction = end - start;
            float magnitude = direction.magnitude;
            direction.Normalize();

            var lhs = point - start;
            float dotP = Mathf.Clamp(Vector3.Dot(lhs, direction), 0f, magnitude);
            return start + direction * dotP;
        }

        public static Vector3 FindNearestPointOnLine(Vector3 start, Vector3 direction, Vector3 point) {
            direction.Normalize();
            var lhs = point - start;
            float dotP = Vector3.Dot(lhs, direction);
            return start + direction * dotP;
        }
    }

}
