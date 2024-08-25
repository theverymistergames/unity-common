using UnityEngine;

namespace MisterGames.Common.Maths {
    
    public static class TrajectoryUtils {
        
        private const int DEFAULT_PREDICTION_ITERATIONS = 5;
        
        public static Vector3 EvaluateTrajectory(Vector3 origin, Vector3 throwVelocity, float time, float gravity) {
            return new Vector3(
                origin.x + throwVelocity.x * time,
                origin.y + throwVelocity.y * time + 0.5f * gravity * time * time,
                origin.z + throwVelocity.z * time
            );
        }

        public static ThrowData GetThrowData(Vector3 origin, Vector3 target, float throwSpeed, float gravity) {
            if (throwSpeed <= 0f) return default;

            var diff = target - origin;
            float dxz = new Vector3(diff.x, 0f, diff.z).magnitude;

            float t;

            if (dxz > 0f) {
                t = dxz / throwSpeed;
            }
            else {
                float dSqr = throwSpeed * throwSpeed + 2f * gravity * diff.y;
                if (dSqr < 0f) return default;
                
                t = (-(Mathf.Sign(diff.y) * throwSpeed) + Mathf.Sign(diff.y) * Mathf.Sqrt(dSqr)) / gravity;
            }

            if (t <= 0f) return default;
            
            var velocity = new Vector3(diff.x / t, (diff.y - 0.5f * gravity * t * t) / t, diff.z / t);
            return new ThrowData(velocity, t);
        }

        public static ThrowData GetThrowDataByHeight(Vector3 origin, Vector3 target, float height, float gravity) {
            var diff = target - origin;
            
            float yTop = diff.y >= 0f ? diff.y : 0f;
            float vy0 = Mathf.Sqrt(-2f * gravity * (yTop + height));
            
            float t = (-vy0 - Mathf.Sqrt(vy0 * vy0 + 2f * gravity * diff.y)) / gravity;
            var velocity = new Vector3(t > 0f ? diff.x / t : 0f, vy0, t > 0f ? diff.z / t : 0f);
            
            return new ThrowData(velocity, t);
        }

        public static ThrowData GetThrowDataPredicted(
            Vector3 origin,
            Vector3 target,
            Vector3 targetVelocity,
            float throwSpeed,
            float gravity,
            int iterations = DEFAULT_PREDICTION_ITERATIONS
        ) {
            var data = GetThrowData(origin, target, throwSpeed, gravity);
            
            for (int i = 0; i < iterations; i++) {
                data = GetThrowData(origin, target + targetVelocity * data.time, throwSpeed, gravity);
            }

            return data;
        }

        public static ThrowData GetThrowDataByHeightPredicted(
            Vector3 origin,
            Vector3 target,
            float height,
            Vector3 velocity,
            float gravity,
            int iterations = DEFAULT_PREDICTION_ITERATIONS
        ) {
            var data = GetThrowDataByHeight(origin, target, height, gravity);
            
            for (int i = 0; i < iterations; i++) {
                data = GetThrowDataByHeight(origin, target + velocity * data.time, height, gravity);
            }

            return data;
        }

        public static Vector3 GetHomingPositionPredicted(
            Vector3 origin,
            Vector3 target,
            Vector3 targetVelocity,
            float maxSpeed,
            int iterations = DEFAULT_PREDICTION_ITERATIONS
        ) {
            if (maxSpeed <= 0f) return target;
            
            var position = target;
            
            for (int i = 0; i < iterations; i++) {
                position = target + targetVelocity * ((origin - position).magnitude / maxSpeed);
            }

            return position;
        }
    }
    
}