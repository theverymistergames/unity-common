using System;
using UnityEngine;
using UnityEngine.Playables;

namespace MisterGames.TweenLib.Playables {

    [Serializable]
    public sealed class RotateTransformBehaviour : PlayableBehaviour {

        public Vector3 startEulerAngles;
        public Vector3 endEulerAngles;
        public bool useLocal = true;
        public bool useEulerInterpolation;
        public AnimationCurve curve = AnimationCurve.Linear(0f, 0f , 1f, 1f);

        public Quaternion GetRotation(Transform transform, float progress) {
            float t = curve.Evaluate(progress);

            var rotation = useEulerInterpolation
                ? Quaternion.Slerp(Quaternion.Euler(startEulerAngles), Quaternion.Euler(endEulerAngles), t)
                : Quaternion.Euler(Vector3.Lerp(startEulerAngles, endEulerAngles, t));

            return useLocal || transform.parent == null
                ? rotation
                : rotation * Quaternion.Inverse(transform.parent.rotation);
        }
    }

}
