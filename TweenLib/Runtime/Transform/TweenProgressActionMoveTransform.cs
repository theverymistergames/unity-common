﻿using System;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.TweenLib {

    [Serializable]
    public sealed class TweenProgressActionMoveTransform : ITweenProgressAction {

        public Transform transform;
        public bool useLocal = true;
        public Vector3 startPosition;
        public Vector3 endPosition;

        public void Initialize(MonoBehaviour owner) { }
        public void DeInitialize() { }

        public void Start() { }

        public void Finish() {
#if UNITY_EDITOR
            if (!Application.isPlaying && transform != null) UnityEditor.EditorUtility.SetDirty(transform);
#endif
        }

        public void OnProgressUpdate(float progress) {
            var value = Vector3.Lerp(startPosition, endPosition, progress);

            if (useLocal) transform.localPosition = value;
            else transform.position = value;
        }
    }

}
