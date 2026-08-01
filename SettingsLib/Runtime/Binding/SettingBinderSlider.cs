using System;
using MisterGames.SettingsLib.Descs;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.SettingsLib.Base {
    
    [Serializable]
    public sealed class SettingBinderSlider : ISettingBinder {

        public Slider slider;

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
            
            _desc.AddListener(OnSettingValueChanged);
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        void ISettingBinder.Unbind() {
            _desc?.RemoveListener(OnSettingValueChanged);
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
            
            _service = null;
            _desc = null;
            _id = null;
        }

        private void OnSettingValueChanged(string id, float value) {
            if (_ignoreValueChange || _desc == null) return;
            
            SetupValue(_desc);
        }

        private void OnSliderValueChanged(float value) {
            if (_ignoreValueChange) return;

            _ignoreValueChange = true;
            _lastSetValue = slider.normalizedValue;
            _desc.SetValue(_service, _id, _lastSetValue);
            _ignoreValueChange = false;
        }

        void ISettingBinder.SetupView(ISettingDesc desc) {
            if (!IsValidSettingDesc(desc, out var d) || slider == null) return;

            _ignoreValueChange = true;
            slider.normalizedValue = d.GetDefaultValue();
            _ignoreValueChange = false;
        }

        public void SetupValue(ISettingDesc desc) {
            if (_desc == null || _service == null || string.IsNullOrEmpty(_id)) return;

            _ignoreValueChange = true;
            slider.normalizedValue = _desc.GetValue(_service, _id);
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