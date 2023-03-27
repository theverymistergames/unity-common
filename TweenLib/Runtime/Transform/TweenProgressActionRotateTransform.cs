using System;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.TweenLib {

    [Serializable]
    public sealed class TweenProgressActionRotateTransform : ITweenProgressAction {

        public Transform transform;
        public Vector3 startEulerAngles;
        public Vector3 endEulerAngles;
        public bool useLocal = true;

        private Quaternion _startRotation;
        private Quaternion _endRotation;

        public void Initialize(MonoBehaviour owner) { }
        public void DeInitialize() { }

        public void Start() {
            _startRotation = Quaternion.Euler(startEulerAngles);
            _endRotation = Quaternion.Euler(endEulerAngles);
        }

        public void Finish() { }

        public void OnProgressUpdate(float progress) {
            var value = Quaternion.Slerp(_startRotation, _endRotation, progress);

            if (useLocal) transform.localRotation = value;
            else transform.rotation = value;
        }
    }

}
