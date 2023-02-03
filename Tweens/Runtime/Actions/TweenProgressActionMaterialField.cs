using System;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.Tweens.Actions {

    [Serializable]
    public sealed class TweenProgressActionMaterialField : ITweenProgressAction {

        [SerializeField] private Renderer _renderer;
        [SerializeField] private string _fieldName;
        [SerializeField] private float _startValue = 0;
        [SerializeField] private float _endValue;
        [SerializeField] private bool _useColor;

        public Renderer Renderer { get => _renderer; set => _renderer = value; }
        public string FieldName { get => _fieldName ; set => _fieldName = value; }
        public float StartValue { get => _startValue; set => _startValue = value; }
        public float EndValue { get => _endValue; set => _endValue = value; }
        public bool UseColor { get => _useColor; set => _useColor = value; }

        private Material _material;
        private Color _color;

        public void Initialize(MonoBehaviour owner) {
            _material = new Material(_renderer.sharedMaterial);

            _renderer.material = _material;
            _color = _material.color;
        }

        public void DeInitialize() { }

        public void OnProgressUpdate(float progress) {
            float value = Mathf.Lerp(_startValue, _endValue, progress);

            if (_useColor) {
                _material.SetColor(_fieldName, value * _color);
                return;
            }

            _material.SetFloat(_fieldName, value);
        }
    }

}
