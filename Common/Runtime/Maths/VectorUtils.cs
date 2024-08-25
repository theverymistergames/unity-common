using System.Runtime.CompilerServices;
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
        public static Vector3 WithX(this Vector3 vector, float x) => new(x, vector.y, vector.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 WithY(this Vector3 vector, float y) => new(vector.x, y, vector.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 WithZ(this Vector3 vector, float z) => new(vector.x, vector.y, z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 WithX(this Vector2 vector, float x) => new(x, vector.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 WithY(this Vector2 vector, float y) => new(vector.x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 WithZ(this Vector2 vector, float z) => new(vector.x, vector.y, z);
        
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
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Divide(this Vector2 a, Vector2 b) {
            return new Vector2(a.x / b.x, a.y / b.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Divide(this Vector2 a, float x, float y) {
            return new Vector2(a.x / x, a.y / y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Divide(this Vector3 a, float x, float y, float z) {
            return new Vector3(a.x / x, a.y / y, a.z / z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Divide(this Vector3 a, Vector3 b) {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 FloorToInt(this Vector2 value) {
            return new Vector2(Mathf.FloorToInt(value.x), Mathf.FloorToInt(value.y));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 FloorToInt(this Vector3 value) {
            return new Vector3(Mathf.FloorToInt(value.x), Mathf.FloorToInt(value.y), Mathf.FloorToInt(value.z));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 CeilToInt(this Vector2 value) {
            return new Vector2(Mathf.CeilToInt(value.x), Mathf.CeilToInt(value.y));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 CeilToInt(this Vector3 value) {
            return new Vector3(Mathf.CeilToInt(value.x), Mathf.CeilToInt(value.y), Mathf.CeilToInt(value.z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Mod(this Vector2 value, float divider) {
            return new Vector2(value.x % divider, value.y % divider);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Mod(this Vector3 value, float divider) {
            return new Vector3(value.x % divider, value.y % divider, value.z % divider);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToEulerAngles180(float eulerAngle) {
            return (eulerAngle + 180f) % 360f - 180f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToEulerAngles180(this Vector2 eulerAngles) {
            return new Vector2(ToEulerAngles180(eulerAngles.x), ToEulerAngles180(eulerAngles.y));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToEulerAngles180(this Vector3 eulerAngles) {
            return new Vector3(ToEulerAngles180(eulerAngles.x), ToEulerAngles180(eulerAngles.y), ToEulerAngles180(eulerAngles.z));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToEulerAngles180(this Quaternion quaternion) {
            return quaternion.eulerAngles.ToEulerAngles180();
        }

        // ---------------- ---------------- Geometry ---------------- ----------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SqrMagnitudeOfProject(Vector3 vector, Vector3 onNormal)
        {
            return Vector3.Project(vector, onNormal).sqrMagnitude;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MagnitudeOfProject(Vector3 vector, Vector3 onNormal)
        {
            return Vector3.Project(vector, onNormal).magnitude;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SignedMagnitudeOfProject(Vector3 vector, Vector3 onNormal)
        {
            var p = Vector3.Project(vector, onNormal);
            return p.magnitude * (Vector3.Dot(p, onNormal) > 0f ? 1f : -1f);
        }
        
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
        
        /// <summary>
        /// Clamp velocity by direction, so velocity projection along the direction will not exceed max speed.
        /// </summary>
        public static Vector3 ClampVelocity(Vector3 velocity, Vector3 direction, float maxSpeed)
        {
            // Velocity is not directed along the direction, do nothing.
            if (Vector3.Dot(velocity, direction) <= 0f)
            {
                return velocity;
            }
            
            var velocityProjection = Vector3.Project(velocity, direction);
            
            // Velocity projection on direction does not exceed max speed, do nothing.
            if (velocityProjection.sqrMagnitude <= maxSpeed * maxSpeed)
            {
                return velocity;
            }
            
            // New velocity is a projection along direction + projection on plane of direction.
            // Plane projection of the velocity is needed to save velocity part that is not related to the direction. 
            return velocityProjection.normalized * maxSpeed + Vector3.ProjectOnPlane(velocity, direction);
        }
        
        /// <summary>
        /// Clamp acceleration to prevent velocity exceeding max speed along the direction of acceleration.
        /// </summary>
        public static Vector3 ClampAcceleration(
            Vector3 acceleration,
            Vector3 velocity,
            float maxSpeed,
            float dt)
        {
            float maxSpeedSqr = maxSpeed * maxSpeed;
            var velocityProjection = Vector3.Project(velocity, acceleration);

            // Velocity projection directed as acceleration and exceeds max speed: return zero force.
            if (Vector3.Dot(velocityProjection, acceleration) > 0f &&
                velocityProjection.sqrMagnitude >= maxSpeedSqr)
            {
                return Vector3.zero;
            }
            
            // Velocity will not exceed max value if acceleration is applied.  
            if ((velocityProjection + dt * acceleration).sqrMagnitude <= maxSpeedSqr)
            {
                return acceleration;
            }
            
            // Recreate acceleration from delta between current and max velocity,
            // so if acceleration is applied, velocity will not exceed max speed. 
            float clamp = (acceleration.normalized * maxSpeed - velocityProjection).magnitude / dt;
            return Vector3.ClampMagnitude(acceleration, clamp);
        }
        
        public static float SmoothExp(this float value, float target, float factor) {
            return value + (target - value) * (1f - Mathf.Exp(-factor));
        }
        
        public static Vector2 SmoothExp(this Vector2 value, Vector2 target, float factor) {
            return value + (target - value) * (1f - Mathf.Exp(-factor));
        }
        
        public static Vector3 SmoothExp(this Vector3 value, Vector3 target, float factor) {
            return value + (target - value) * (1f - Mathf.Exp(-factor));
        }
    }

}
