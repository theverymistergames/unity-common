using System;
using UnityEngine;
using UnityEngine.Playables;

namespace MisterGames.TweenLib.Playables {

    [Serializable]
    public sealed class MoveTransformBehaviour : PlayableBehaviour {

        public Vector3 startPosition;
        public Vector3 endPosition;
        public bool useLocal = true;
        public AnimationCurve curve = AnimationCurve.Linear(0f, 0f , 1f, 1f);

        public Vector3 GetPosition(Transform transform, float progress) {
            float t = curve.Evaluate(progress);
            var position = Vector3.Lerp(startPosition, endPosition, t);

            return useLocal ? position : transform.InverseTransformPoint(position);
        }
    }

}
