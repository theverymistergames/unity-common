using System;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.TweenLib {

    [Serializable]
    public sealed class TweenProgressActionScaleTransform : ITweenProgressAction {

        public Transform transform;
        public Vector3 startLocalScale;
        public Vector3 endLocalScale;

        public void Initialize(MonoBehaviour owner) { }
        public void DeInitialize() { }

        public void Start() { }
        public void Finish() { }

        public void OnProgressUpdate(float progress) {
            transform.localScale = Vector3.Lerp(startLocalScale, endLocalScale, progress);
        }
    }

}
