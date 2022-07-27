using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MisterGames.Common.Maths {

    public static class VectorUtils {

        // ---------------- ---------------- Equality ---------------- ----------------
        
        public static bool IsNearlyZero(this Vector3 vector) {
            return vector.x.IsNearlyZero() && vector.y.IsNearlyZero() && vector.z.IsNearlyZero();
        }

        public static bool IsNearlyZero(this Vector2 vector) {
            return vector.x.IsNearlyZero() && vector.y.IsNearlyZero();
        }
        
        public static bool IsEqual(this Vector3 vector, Vector3 other) {
            return vector.x.IsNearlyEqual(other.x) && vector.y.IsNearlyEqual(other.y) && vector.z.IsNearlyEqual(other.z);
        }
        
        public static bool IsEqual(this Vector2 vector, Vector2 other) {
            return vector.x.IsNearlyEqual(other.x) && vector.y.IsNearlyEqual(other.y);
        }
        
        // ---------------- ---------------- Rotation ---------------- ----------------
        
        public static Vector3 Rotate(this Vector3 vector, Quaternion rotation) {
            return rotation * vector;
        }
        
        public static Vector2 Rotate(this Vector2 vector, Quaternion rotation) {
            return rotation * vector;
        }
        
        public static Vector3 RotateFromTo(this Vector3 vector, Vector3 from, Vector3 to) {
            return Quaternion.FromToRotation(from, to) * vector;
        }
        
        public static Vector2 RotateFromTo(this Vector2 vector, Vector2 from, Vector2 to) {
            return Quaternion.FromToRotation(from, to) * vector;
        }
        
        // ---------------- ---------------- Modification ---------------- ----------------
        
        public static Vector3 Inverted(this Vector3 vector) {
            return vector * -1f;
        }

        public static Vector2 Inverted(this Vector2 vector) {
            return vector * -1f;
        }
        
        public static Vector3 WithX(this Vector3 vector, float x) {
            return new Vector3(x, vector.y, vector.z);
        }
        
        public static Vector3 WithY(this Vector3 vector, float y) {
            return new Vector3(vector.x, y, vector.z);
        }

        public static Vector3 WithZ(this Vector3 vector, float z) {
            return new Vector3(vector.x, vector.y, z);
        }
        
        public static Vector2 WithX(this Vector2 vector, float x) {
            return new Vector2(x, vector.y);
        }
        
        public static Vector2 WithY(this Vector2 vector, float y) {
            return new Vector2(vector.x, y);
        }

        // ---------------- ---------------- Utils ---------------- ----------------

        public static float DistanceTo(this Vector3 first, Vector3 second) {
            return Vector3.Distance(first, second);
        }

        public static float SqrDistanceTo(this Vector3 first, Vector3 second) {
            return (second - first).sqrMagnitude;
        }

        public static float DistanceTo(this Vector2 first, Vector2 second) {
            return Vector2.Distance(first, second);
        }

        public static float SqrDistanceTo(this Vector2 first, Vector2 second) {
            return (second - first).sqrMagnitude;
        }

        public static Vector3 FindNearestPointOnSegment(Vector3 start, Vector3 end, Vector3 point) {
            var direction = (end - start);
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
