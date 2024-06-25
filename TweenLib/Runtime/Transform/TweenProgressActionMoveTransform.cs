using System;
using MisterGames.Common.Maths;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib {

    [Serializable]
    public sealed class TweenProgressActionMoveTransform : ITweenProgressAction {

        public Transform transform;
        public bool useLocal = true;
        public Vector3 startPosition;
        public Vector3 endPosition;
        public float curvature;
        public Vector3 curvatureAxis = Vector3.up;

        public void OnProgressUpdate(float progress) {
            var value = Evaluate(progress);
            
            if (useLocal) transform.localPosition = value;
            else transform.position = value;

#if UNITY_EDITOR
            if (!Application.isPlaying && transform != null) UnityEditor.EditorUtility.SetDirty(transform);
#endif
        }

        private Vector3 Evaluate(float progress) {
            if (curvature.IsNearlyZero()) {
                return Vector3.LerpUnclamped(startPosition, endPosition, progress);
            }
            
            var rot = useLocal ? transform.localRotation : transform.rotation;
            var curvaturePoint = BezierExtensions.GetCurvaturePoint(
                startPosition,
                endPosition,
                Quaternion.LookRotation(curvatureAxis), 
                curvature
            );

            return BezierExtensions.EvaluateBezier3Points(startPosition, curvaturePoint, endPosition, progress);
        }
    }

}
