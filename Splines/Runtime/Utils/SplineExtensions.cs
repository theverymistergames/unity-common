using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace MisterGames.Splines.Utils {

    public static class SplineExtensions {

        public static Vector3 GetNearestPoint(this SplineContainer splineContainer, Vector3 point, out float t, int splineIndex = 0) {
            var localPoint = splineContainer.transform.InverseTransformPoint(point);

            SplineUtility.GetNearestPoint(
                splineContainer[splineIndex],
                localPoint,
                out var nearestPoint,
                out t
            );

            return splineContainer.transform.TransformPoint(nearestPoint);
        }

        public static float MoveAlongSpline(this SplineContainer splineContainer, Vector3 delta, float t) {
            delta = splineContainer.transform.InverseTransformDirection(delta);
            return MoveAlongSpline(splineContainer.Spline, delta, t);
        }

        public static float MoveAlongSpline(this ISpline spline, Vector3 delta, float t) {
            float splineLength = spline.GetLength();
            t = math.clamp(t, 0f, 1f);

            if (splineLength <= math.EPSILON ||
                !spline.Evaluate(t, out var position, out var tangent, out _)
            ) {
                return t;
            }

            var projection = math.project(delta, tangent);
            var targetPoint = position + projection;
            float distance = math.length(projection);

            float deltaT = splineLength > 0f ? distance / splineLength : 0f;
            float forwardT = math.clamp(t + deltaT, 0f, 1f);
            float backwardT = math.clamp(t - deltaT, 0f, 1f);

            spline.Evaluate(forwardT, out var forwardPosition, out _, out _);
            spline.Evaluate(backwardT, out var backwardPosition, out _, out _);

            return math.distancesq(targetPoint, forwardPosition) < math.distancesq(targetPoint, backwardPosition)
                ? forwardT
                : backwardT;
        }
    }

}
