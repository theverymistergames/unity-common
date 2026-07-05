using System;
using MisterGames.UI.Components;
using UnityEngine;

namespace MisterGames.SettingsLib.Base {
    
    [Serializable]
    public sealed class SettingBinderListed : ISettingBinder {

        public UiList uiList;

        private ISettingsService _service;
        private ISettingDescListed _desc;
        private string _label;

        void ISettingBinder.Bind(ISettingsService service, ISettingDesc desc, string label) {
            if (!IsValidSettingDesc(desc, out var descListed)) return;

            _service = service;
            _desc = descListed;
            _label = label;
            uiList.OnSelectedIndexChanged += OnSelectedIndexChanged;
        }

        void ISettingBinder.Unbind() {
            _service = null;
            _desc = null;
            _label = null;
            uiList.OnSelectedIndexChanged -= OnSelectedIndexChanged;
        }

        void ISettingBinder.SetupView(ISettingDesc desc) {
            if (!IsValidSettingDesc(desc, out var descListed) || uiList == null) return;

            int count = descListed.GetCount();
            uiList.SetElementsCount(count);
            
            for (int i = 0; i < count; i++) {
                uiList.SetElement(i, descListed.GetValue(i));
            }
        }

        void ISettingBinder.SetupValue() {
            if (_desc == null || _service == null || string.IsNullOrEmpty(_label)) {
                return;
            }
            
            uiList.SelectIndex(_desc.GetIndex(_service, _label));
        }

        private void OnSelectedIndexChanged(int index) {
            if (_desc == null || _service == null || string.IsNullOrEmpty(_label)) { 
                return;
            }
            
            _desc.SetIndex(_service, _label, index);
        }

        private bool IsValidSettingDesc(ISettingDesc desc, out ISettingDescListed descListed) {
            if (desc is not ISettingDescListed d) {
                Debug.LogError($"Setting binder {GetType().Name} requires a setting desc that implements {nameof(ISettingDescListed)}. " +
                               $"Provided invalid desc of type {desc?.GetType().Name}.");
                descListed = null;
                return false;
            }
            descListed = d;
            return true;
        }
    }
    
}