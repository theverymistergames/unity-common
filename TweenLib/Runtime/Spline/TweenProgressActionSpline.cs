using System;
using MisterGames.Common.Attributes;
using MisterGames.Tweens;
using UnityEngine;
using UnityEngine.Splines;

namespace MisterGames.TweenLib.Spline {
    
    [Serializable]
    public sealed class TweenProgressActionSpline : ITweenProgressAction {

        public Transform target;
        public SplineContainer splineContainer;
        [MinMaxSlider(0f, 1f)] public Vector2 bounds = new Vector2(0f, 1f);
        public bool rotate;

        public void OnProgressUpdate(float progress) {
            float diff = bounds.y - bounds.x;
            float t = diff > 0f ? bounds.x + progress / diff : 0f;

            splineContainer.Evaluate(t, out var position, out var tangent, out var up);
            
            var rotation = rotate ? Quaternion.LookRotation(tangent, up) : target.rotation;
            target.SetPositionAndRotation(position, rotation);
        }
    }
    
}