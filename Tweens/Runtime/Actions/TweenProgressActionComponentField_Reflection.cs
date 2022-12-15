using System;
using System.Reflection;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.Tweens.Actions {

    [Serializable]
    [Obsolete("Using TweenProgressActionComponentField_Reflection, must be replaced later!")]
    public sealed class TweenProgressActionComponentField_Reflection : ITweenProgressAction {

        [SerializeField] private Component _component;

        [SerializeField] private string _fieldName;
        [SerializeField] private float _startValue = 0;
        [SerializeField] private float _endValue = 1;

        private FieldInfo _field;
        private PropertyInfo _property;
        
        private BindingFlags _flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

        public void Initialize(MonoBehaviour owner) {
            Debug.LogError("Using TweenProgressActionComponentField_Reflection, must be replaced later!");

            _field = _component.GetType().GetField(_fieldName, _flags);
            if (_field == null) _property = _component.GetType().GetProperty(_fieldName, _flags);
        }

        public void DeInitialize() { }

        public void OnProgressUpdate(float progress) {
            float value = Mathf.Lerp(_startValue, _endValue, progress);

            if (_field != null) {
                _field.SetValue(_component, value);
                return;
            }

            _property.SetValue(_component, value);
        }
    }
}
