using System;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.TweenLib {

    [Serializable]
    public sealed class TweenProgressActionMoveTransform : ITweenProgressAction {

        public Transform transform;
        public Vector3 startPosition;
        public Vector3 endPosition;
        public bool useLocal = true;

        public void Initialize(MonoBehaviour owner) { }

        public void DeInitialize() { }

        public void OnProgressUpdate(float progress) {
            var value = Vector3.Lerp(startPosition, endPosition, progress);

            if (useLocal) transform.localPosition = value;
            else transform.position = value;
        }
    }

}
