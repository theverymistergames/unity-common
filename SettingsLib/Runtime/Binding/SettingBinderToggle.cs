using System;
using MisterGames.SettingsLib.Descs;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.SettingsLib.Base {
    
    [Serializable]
    public sealed class SettingBinderToggle : ISettingBinder {

        public Toggle toggle;

        private ISettingsService _service;
        private ISettingDescValued<bool> _desc;
        private string _id;
        private bool _lastSetValue;
        private bool _ignoreValueChange;

        void ISettingBinder.Bind(ISettingsService service, ISettingDesc desc, string id) {
            if (!IsValidSettingDesc(desc, out var descListed)) return;

            _service = service;
            _desc = descListed;
            _id = id;
            
            _desc.AddListener(OnSettingValueChanged);
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        void ISettingBinder.Unbind() {
            _desc?.RemoveListener(OnSettingValueChanged);
            toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
            
            _service = null;
            _desc = null;
            _id = null;
        }

        private void OnSettingValueChanged(string id, bool value) {
            if (_ignoreValueChange || _desc == null) return;
            
            SetupValue(_desc);
        }

        private void OnToggleValueChanged(bool value) {
            if (_ignoreValueChange || value == _lastSetValue) return;

            _ignoreValueChange = true;
            _lastSetValue = value;
            _desc.SetValue(_service, _id, _lastSetValue);
            _ignoreValueChange = false;
        }

        void ISettingBinder.SetupView(ISettingDesc desc) {
            if (!IsValidSettingDesc(desc, out var d) || toggle == null) return;

            _ignoreValueChange = true;
            toggle.isOn = d.GetDefaultValue();
            _ignoreValueChange = false;
        }

        public void SetupValue(ISettingDesc desc) {
            if (_desc == null || _service == null || string.IsNullOrEmpty(_id)) return;

            _ignoreValueChange = true;
            toggle.isOn = _desc.GetValue(_service, _id);
            _ignoreValueChange = false;
        }

        private bool IsValidSettingDesc(ISettingDesc desc, out ISettingDescValued<bool> descValued) {
            if (desc is not ISettingDescValued<bool> d) {
                Debug.LogError($"Setting binder {GetType().Name} requires a setting desc that implements {nameof(ISettingDescValued<bool>)}. " +
                               $"Provided invalid desc of type {desc?.GetType().Name}.");
                descValued = null;
                return false;
            }
            descValued = d;
            return true;
        }
    }
    
}