using System;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.TweenLib {

    [Serializable]
    public sealed class TweenProgressActionScaleTransform : ITweenProgressAction {

        public Transform transform;
        public Vector3 startScale;
        public Vector3 endScale;

        public void Initialize(MonoBehaviour owner) { }

        public void DeInitialize() { }

        public void OnProgressUpdate(float progress) {
            var value = Vector3.Lerp(startScale, endScale, progress);
            
            transform.localScale = value;
        }
    }

}
