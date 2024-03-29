﻿using System;
using System.Reflection;
using MisterGames.Tweens;
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

        private static BindingFlags _bindingFlags =
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

        public void OnProgressUpdate(float progress) {
            if (_field == null && _property == null) {
                Debug.LogWarning($"Using {nameof(TweenProgressActionComponentField_Reflection)}. " +
                                 $"This class is for debug purposes only, " +
                                 $"don`t forget to remove it in release mode.");

                _field = component.GetType().GetField(fieldName, _bindingFlags);
                if (_field == null) _property = component.GetType().GetProperty(fieldName, _bindingFlags);
            }

            float value = Mathf.Lerp(startValue, endValue, progress);

            if (_field != null) {
                _field.SetValue(component, value);
            }
            else {
                _property?.SetValue(component, value);
            }

#if UNITY_EDITOR
            if (!Application.isPlaying && component != null) UnityEditor.EditorUtility.SetDirty(component);
#endif
        }
    }

}
