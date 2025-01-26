using System.Runtime.CompilerServices;
using UnityEngine;

namespace MisterGames.Common.Maths {

    public static class VectorUtils {

        // ---------------- ---------------- Modification ---------------- ----------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 WithX(this Vector2 vector, float x) => new(x, vector.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 WithY(this Vector2 vector, float y) => new(vector.x, y);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 WithX(this Vector3 vector, float x) => new(x, vector.y, vector.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 WithY(this Vector3 vector, float y) => new(vector.x, y, vector.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 WithZ(this Vector3 vector, float z) => new(vector.x, vector.y, z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 WithXY(this Vector3 vector, float x, float y) => new(x, y, vector.z);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 WithXZ(this Vector3 vector, float x, float z) => new(x, vector.y, z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 WithYZ(this Vector3 vector, float y, float z) => new(vector.x, y, z);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 WithoutX(this Vector3 vector) => new(vector.y, vector.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 WithoutY(this Vector3 vector) => new(vector.x, vector.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 WithoutZ(this Vector3 vector) => new(vector.x, vector.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToXYZ(this Vector2 vector, float z) => new(vector.x, vector.y, z);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToXZY(this Vector2 vector, float z) => new(vector.x, z, vector.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToYXZ(this Vector2 vector, float z) => new(vector.y, vector.x, z);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToYZX(this Vector2 vector, float z) => new(vector.y, z, vector.x);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToZXY(this Vector2 vector, float z) => new(z, vector.x, vector.y);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToZYX(this Vector2 vector, float z) => new(z, vector.y, vector.x);
        
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
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 GetNearestAngle(Vector2 value, Vector2 target) {
            return new Vector2(GetNearestAngle(value.x, target.x), GetNearestAngle(value.y, target.y));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetNearestAngle(Vector3 value, Vector3 target) {
            return new Vector3(GetNearestAngle(value.x, target.x), GetNearestAngle(value.y, target.y), GetNearestAngle(value.z, target.z));
        }
        
        public static float GetNearestAngle(float value, float target) {
            value %= 360f;
            float t = Mathf.FloorToInt(target / 360f) + value > 0f ? -1f : 0f;
            
            float p0 = t * 360f + value;
            float p1 = (t + 1f) * 360f + value;
            float p2 = (t + 2f) * 360f + value;
            float p01 = Mathf.Abs(p0 - target) < Mathf.Abs(p1 - target) ? p0 : p1;
            
            return Mathf.Abs(p01 - target) < Mathf.Abs(p2 - target) ? p01 : p2;
        }

        // ---------------- ---------------- Geometry ---------------- ----------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SqrMagnitudeOfProject(Vector3 vector, Vector3 onNormal) {
            return Vector3.Project(vector, onNormal).sqrMagnitude;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MagnitudeOfProject(Vector3 vector, Vector3 onNormal) {
            return Vector3.Project(vector, onNormal).magnitude;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SignedMagnitudeOfProject(Vector3 vector, Vector3 onNormal) {
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
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SmoothExp(this float value, float target, float factor) {
            return value + (target - value) * (1f - Mathf.Exp(-factor));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 SmoothExp(this Vector2 value, Vector2 target, float factor) {
            return value + (target - value) * (1f - Mathf.Exp(-factor));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SmoothExp(this Vector3 value, Vector3 target, float factor) {
            return value + (target - value) * (1f - Mathf.Exp(-factor));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion Slerp(this Quaternion value, Quaternion target, float factor) {
            return Quaternion.Slerp(value, target, factor);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion SlerpNonZero(this Quaternion value, Quaternion target, float factor, float dt) {
            return factor > 0f ? Quaternion.Slerp(value, target, factor * dt) : target;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SmoothExpNonZero(this float value, float target, float factor, float dt) {
            return factor > 0f ? value + (target - value) * (1f - Mathf.Exp(-factor * dt)) : target;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 SmoothExpNonZero(this Vector2 value, Vector2 target, float factor, float dt) {
            return factor > 0f ? value + (target - value) * (1f - Mathf.Exp(-factor * dt)) : target;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SmoothExpNonZero(this Vector3 value, Vector3 target, float factor, float dt) {
            return factor > 0f ? value + (target - value) * (1f - Mathf.Exp(-factor * dt)) : target;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SmoothExpAngle(this float value, float target, float factor) {
            return SmoothExp(value, GetNearestAngle(target, value), factor);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 SmoothExpAngle(this Vector2 value, Vector2 target, float factor) {
            return new Vector2(
                SmoothExpAngle(value.x, target.x, factor), 
                SmoothExpAngle(value.y, target.y, factor)
            );
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SmoothExpAngle(this Vector3 value, Vector3 target, float factor) {
            return new Vector3(
                SmoothExpAngle(value.x, target.x, factor), 
                SmoothExpAngle(value.y, target.y, factor), 
                SmoothExpAngle(value.z, target.z, factor)
            );
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SmoothExpAngleNonZero(this float value, float target, float factor, float dt) {
            return SmoothExpNonZero(value, GetNearestAngle(target, value), factor, dt);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 SmoothExpAngleNonZero(this Vector2 value, Vector2 target, float factor, float dt) {
            return new Vector2(
                SmoothExpAngleNonZero(value.x, target.x, factor, dt), 
                SmoothExpAngleNonZero(value.y, target.y, factor, dt)
            );
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SmoothExpAngleNonZero(this Vector3 value, Vector3 target, float factor, float dt) {
            return new Vector3(
                SmoothExpAngleNonZero(value.x, target.x, factor, dt), 
                SmoothExpAngleNonZero(value.y, target.y, factor, dt), 
                SmoothExpAngleNonZero(value.z, target.z, factor, dt)
            );
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Abs(this Vector2 value) {
            return new Vector2(Mathf.Abs(value.x), Mathf.Abs(value.y));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Abs(this Vector3 value) {
            return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Min(Vector2 a, Vector2 b) {
            return new Vector2(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Min(Vector3 a, Vector3 b) {
            return new Vector3(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Min(a.z, b.z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Max(Vector2 a, Vector2 b) {
            return new Vector2(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Max(Vector3 a, Vector3 b) {
            return new Vector3(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y), Mathf.Max(a.z, b.z));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Sign(this Vector2 value) {
            return new Vector2(Mathf.Sign(value.x), Mathf.Sign(value.y));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Sign(this Vector3 value) {
            return new Vector3(Mathf.Sign(value.x), Mathf.Sign(value.y), Mathf.Sign(value.z));
        }
    }

}
