using System;
using System.Reflection;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.TweenLib {

    [Serializable]
    [Obsolete("Using TweenProgressActionComponentField_Reflection, must be replaced later!")]
    public sealed class TweenProgressActionComponentField_Reflection : ITweenProgressAction {

        public Component component;
        public string fieldName;
        public float startValue = 0;
        public float endValue = 1;

        private FieldInfo _field;
        private PropertyInfo _property;
        
        private BindingFlags _flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

        public void Initialize(MonoBehaviour owner) {
            Debug.LogWarning($"Using {nameof(TweenProgressActionComponentField_Reflection)} on game object {owner.name}, must be replaced later!");

            _field = component.GetType().GetField(fieldName, _flags);
            if (_field == null) _property = component.GetType().GetProperty(fieldName, _flags);
        }

        public void DeInitialize() { }

        public void OnProgressUpdate(float progress) {
            float value = Mathf.Lerp(startValue, endValue, progress);

            if (_field != null) {
                _field.SetValue(component, value);
                return;
            }

            _property.SetValue(component, value);
        }
    }
}
