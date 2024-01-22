using System;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.TweenLib {

    [Serializable]
    public sealed class TweenProgressActionScaleTransform : ITweenProgressAction {

        public Transform transform;
        public Vector3 startLocalScale;
        public Vector3 endLocalScale;

        public void OnProgressUpdate(float progress) {
            transform.localScale = Vector3.Lerp(startLocalScale, endLocalScale, progress);

#if UNITY_EDITOR
            if (!Application.isPlaying && transform != null) UnityEditor.EditorUtility.SetDirty(transform);
#endif
        }
    }

}
