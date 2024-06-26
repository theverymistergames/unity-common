﻿using System;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib {

    [Serializable]
    public sealed class TweenProgressActionRotateTransform : ITweenProgressAction {

        public Transform transform;
        public Vector3 startEulerAngles;
        public Vector3 endEulerAngles;
        public bool useLocal = true;
        public bool useEulerInterpolation;

        public void OnProgressUpdate(float progress) {
            var value = useEulerInterpolation
                ? Quaternion.Euler(Vector3.LerpUnclamped(startEulerAngles, endEulerAngles, progress))
                : Quaternion.SlerpUnclamped(Quaternion.Euler(startEulerAngles), Quaternion.Euler(endEulerAngles), progress);

            if (useLocal) transform.localRotation = value;
            else transform.rotation = value;

#if UNITY_EDITOR
            if (!Application.isPlaying && transform != null) UnityEditor.EditorUtility.SetDirty(transform);
#endif
        }
    }

}
