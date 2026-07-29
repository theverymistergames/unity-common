using System;
using MisterGames.Common.Maths;
using MisterGames.SettingsLib.Descs;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.SettingsLib.Base {
    
    [Serializable]
    public sealed class SettingBinderSlider : ISettingBinder {

        public Slider slider;
        [Min(0f)] public float sliderValue0;
        [Min(0f)] public float sliderValue1 = 1f;
        [Min(0f)] public float epsilon = 0.01f;

        private ISettingsService _service;
        private ISettingDescValued<float> _desc;
        private string _id;
        private float _lastSetValue;
        private bool _ignoreValueChange;

        void ISettingBinder.Bind(ISettingsService service, ISettingDesc desc, string id) {
            if (!IsValidSettingDesc(desc, out var descListed)) return;

            _service = service;
            _desc = descListed;
            _id = id;
            
            slider.onValueChanged.AddListener(OnValueChanged);
            OnValueChanged(slider.value);
        }

        void ISettingBinder.Unbind() {
            _service = null;
            _desc = null;
            _id = null;
            
            slider.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(float value) {
            if (_ignoreValueChange || value.IsNearlyEqual(_lastSetValue, epsilon)) return;
            
            _lastSetValue = GetSliderNormalizedValue(slider.value);
            _desc.SetValue(_service, _id, _lastSetValue);
        }

        private float GetSliderNormalizedValue(float input) {
            return Mathf.InverseLerp(sliderValue0, sliderValue1, input);
        }
        
        private float GetSliderInput(float output) {
            return Mathf.Lerp(sliderValue0, sliderValue1, output);
        }

        void ISettingBinder.SetupView(ISettingDesc desc) {
            if (!IsValidSettingDesc(desc, out var d) || slider == null) return;

            slider.SetValueWithoutNotify(GetSliderInput(d.GetDefaultValue()));
        }

        void ISettingBinder.SetupValue(ISettingDesc desc) {
            if (_desc == null || _service == null || string.IsNullOrEmpty(_id)) return;

            _ignoreValueChange = true;
            slider.value = GetSliderInput(_desc.GetValue(_service, _id));
            _ignoreValueChange = false;
        }

        private bool IsValidSettingDesc(ISettingDesc desc, out ISettingDescValued<float> descValued) {
            if (desc is not ISettingDescValued<float> d) {
                Debug.LogError($"Setting binder {GetType().Name} requires a setting desc that implements {nameof(ISettingDescValued<float>)}. " +
                               $"Provided invalid desc of type {desc?.GetType().Name}.");
                descValued = null;
                return false;
            }
            descValued = d;
            return true;
        }
    }
    
}