using System.Runtime.CompilerServices;
using UnityEngine;

namespace MisterGames.Common.Maths {
    
    public static class RandomExtensions {
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InRange(this float value, Vector2 range) {
            return value >= range.x && value <= range.y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetRandomInRange(this Vector2 value) {
            return Random.Range(value.x, value.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetRandomInRange(this Vector2Int value) {
            return Random.Range(value.x, value.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 OnUnitCircle() {
            var dir = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up);
            return new Vector2(dir.x, dir.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 OnUnitCircle(Vector3 axis) {
            return Quaternion.AngleAxis(Random.Range(-180f, 180f), axis) * axis.Orthogonal();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetRandomSign() {
            return Random.Range(0, 2) * 2 - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 GetRandomSign2() {
            return new Vector2(GetRandomSign(), GetRandomSign());
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetRandomSign3() {
            return new Vector3(GetRandomSign(), GetRandomSign(), GetRandomSign());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 GetRandomPointInBox(Vector2 halfSize) {
            halfSize = halfSize.Abs();
            return new Vector2(
                Random.Range(-halfSize.x, halfSize.x),
                Random.Range(-halfSize.y, halfSize.y)
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetRandomPointInBox(Vector3 halfSize) {
            halfSize = halfSize.Abs();
            return new Vector3(
                Random.Range(-halfSize.x, halfSize.x), 
                Random.Range(-halfSize.y, halfSize.y), 
                Random.Range(-halfSize.z, halfSize.z)
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 GetRandomPointInBox(Vector2 halfSize, Vector2 excludedCenter) {
            return PlacePointInBounds(GetRandomPointInBox(halfSize), halfSize, excludedCenter);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetRandomPointInBox(Vector3 halfSize, Vector3 excludedCenter) {
            return PlacePointInBounds(GetRandomPointInBox(halfSize), halfSize, excludedCenter);
        }
        
        public static Vector2 PlacePointInBounds(Vector2 point, Vector2 halfSize, Vector2 halfExcludeCenter = default) {
            halfSize = halfSize.Abs();
            halfExcludeCenter = VectorUtils.Min(halfExcludeCenter.Abs(), halfSize);
            
            var sign = point.Sign();
            point = VectorUtils.Min(point.Abs(), halfSize);
            
            if (point.x < halfExcludeCenter.x && point.y < halfExcludeCenter.y) {
                int i = Random.Range(0, 2);
                point[i] = halfSize[i] + point[i] / halfExcludeCenter[i] * (halfExcludeCenter[i] - halfSize[i]);
            }

            return point.Multiply(sign);
        }

        public static Vector3 PlacePointInBounds(Vector3 point, Vector3 halfSize, Vector3 halfExcludeCenter = default) {
            halfSize = halfSize.Abs();
            halfExcludeCenter = VectorUtils.Min(halfExcludeCenter.Abs(), halfSize);
            
            var sign = point.Sign();
            point = VectorUtils.Min(point.Abs(), halfSize);

            if (point.x < halfExcludeCenter.x && point.y < halfExcludeCenter.y && point.z < halfExcludeCenter.z) {
                int i = Random.Range(0, 3);
                point[i] = halfSize[i] + point[i] / halfExcludeCenter[i] * (halfExcludeCenter[i] - halfSize[i]);
            }

            return point.Multiply(sign);
        }
    }
    
}