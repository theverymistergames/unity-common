using System;
using UnityEngine;
using UnityEngine.Playables;

namespace MisterGames.TweenLib.Playables {

    [Serializable]
    public sealed class ScaleTransformBehaviour : PlayableBehaviour {

        public Vector3 startLocalScale = Vector3.one;
        public Vector3 endLocalScale = Vector3.one;
        public AnimationCurve curve = AnimationCurve.Linear(0f, 0f , 1f, 1f);

        public Vector3 GetScale(float progress) {
            float t = curve.Evaluate(progress);
            return Vector3.Lerp(startLocalScale, endLocalScale, t);
        }
    }

}
