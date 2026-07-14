using System;
using MisterGames.Common.Maths;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib {

    [Serializable]
    public sealed class TweenProgressActionMoveTransformByOrientation : ITweenProgressAction {

        public Transform transform;
        public Transform orientation;
        public bool useLocalPosition = true;
        public bool useLocalOrientation = true;
        [Range(0f, 180f)] public float maxInfluenceAngle;
        public Data[] data;

        [Serializable]
        public struct Data {
            public Vector3 orientation;
            public Vector3 startPosition;
            public Vector3 endPosition;
        }
        
        public void OnProgressUpdate(float progress) {
            var value = Evaluate(progress);
            
            if (useLocalPosition) transform.localPosition = value;
            else transform.position = value;

#if UNITY_EDITOR
            if (!Application.isPlaying && transform != null) UnityEditor.EditorUtility.SetDirty(transform);
#endif
        }

        private Vector3 Evaluate(float progress) {
            var currentRot = useLocalOrientation ? orientation.localRotation : orientation.rotation;
            var start = Vector3.zero;
            var end = Vector3.zero;
            float totalWeight = 0f;

            for (int i = 0; i < data?.Length; i++) {
                ref var d = ref data[i];
                var orient = Quaternion.Euler(d.orientation);
                float angle = Quaternion.Angle(currentRot, orient);
                float weight = Mathf.Clamp01(1f - angle / maxInfluenceAngle);
                if (weight <= 0f) continue;

                start += d.startPosition * weight;
                end += d.endPosition * weight;
                
                totalWeight += weight;
            }

            if (totalWeight > 1f) {
                start /= totalWeight;
                end /= totalWeight;
            }

            return Vector3.Lerp(start, end, progress);
        }
    }

}
