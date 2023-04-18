using System;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.TweenLib {

    [Serializable]
    public sealed class TweenProgressActionMaterialField : ITweenProgressAction {

        public Renderer renderer;
        public string fieldName;
        public float startValue = 0;
        public float endValue;
        public bool useColor;

        private Material _material;
        private Color _color;

        public void Initialize(MonoBehaviour owner) {
            _material = new Material(renderer.sharedMaterial);

            renderer.material = _material;
            _color = _material.color;
        }

        public void DeInitialize() { }

        public void Start() { }

        public void Finish() {
#if UNITY_EDITOR
            if (!Application.isPlaying && renderer != null) UnityEditor.EditorUtility.SetDirty(renderer);
#endif
        }

        public void OnProgressUpdate(float progress) {
            float value = Mathf.Lerp(startValue, endValue, progress);

            if (useColor) {
                _material.SetColor(fieldName, value * _color);
                return;
            }

            _material.SetFloat(fieldName, value);
        }
    }

}
