using System;
using UnityEngine;

namespace MisterGames.Common.Maths {
    
    public static class BezierExtensions {

        private const float DEFAULT_STEP = 0.1f;

        public static Vector3 GetCurvaturePoint(Vector3 origin, Vector3 target, Quaternion targetOrientation, float curvature) {
            var dir = origin - target;
            var halfDir = target + dir * 0.5f;
            
            var corner0 = target + Vector3.ProjectOnPlane(dir, targetOrientation * Vector3.right);
            var corner1 = target + Vector3.Project(corner0 - target, targetOrientation * Vector3.forward);
            var corner = (corner0 + corner1) * 0.5f;
            
            var curvePoint = Vector3.LerpUnclamped(halfDir, corner, curvature);

            if (Vector3.Dot(targetOrientation * Vector3.forward, dir) < 0f) {
                curvePoint += 2f * (halfDir - curvePoint);
            }

            return curvePoint;
        }
        
        public static Vector3 EvaluateBezier3Points(Vector3 p0, Vector3 p1, Vector3 p2, float t) {
            return (1f - t) * (1f - t) * p0 + 
                   2f * t * (1f - t) * p1 + 
                   t * t * p2;
        }

        public static Vector3 EvaluateBezier4Points(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
            return (1f - t) * (1f - t) * (1f - t) * p0 + 
                   3f * t * (1f - t) * (1f - t) * p1 + 
                   3f * t * t * (1f - t) * p2 +
                   t * t * t * p3;
        }

        public static float GetBezier3PointsLength(Vector3 p0, Vector3 p1, Vector3 p2, float step = DEFAULT_STEP) {
            return GetCurveLength(
                data: (p0, p1, p2), 
                getPoint: (data, t) => EvaluateBezier3Points(data.p0, data.p1, data.p2, t), 
                step
            );
        }

        public static float GetBezier4PointsLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float step = DEFAULT_STEP) {
            return GetCurveLength(
                data: (p0, p1, p2, p3), 
                getPoint: (data, t) => EvaluateBezier4Points(data.p0, data.p1, data.p2, data.p3, t), 
                step
            );
        }

        public static float GetCurveLength<T>(T data, Func<T, float, Vector3> getPoint, float step = DEFAULT_STEP) {
            if (step <= 0f) step = DEFAULT_STEP;
            
            float length = 0f;

            for (float t = step; t <= 1f; t += step) {
                var p0 = getPoint.Invoke(data, t - step);
                var p1 = getPoint.Invoke(data, t);

                length += Vector3.Distance(p0, p1);
            }

            return length;
        }
    }
    
}