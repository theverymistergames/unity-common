using System;
using MisterGames.UI.Components;
using UnityEngine;

namespace MisterGames.SettingsLib.Base {
    
    [Serializable]
    public sealed class SettingBinderListed : ISettingBinder {

        public UiList uiList;

        private ISettingsService _service;
        private ISettingDescListed _desc;
        private string _id;

        void ISettingBinder.Bind(ISettingsService service, ISettingDesc desc, string id) {
            if (!IsValidSettingDesc(desc, out var descListed)) return;

            _service = service;
            _desc = descListed;
            _id = id;
            uiList.OnSelectedIndexChanged += OnSelectedIndexChanged;
        }

        void ISettingBinder.Unbind() {
            _service = null;
            _desc = null;
            _id = null;
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

        void ISettingBinder.SetupValue(ISettingDesc desc) {
            if (!IsValidSettingDesc(desc, out var descListed) || _service == null || string.IsNullOrEmpty(_id)) {
                return;
            }
            
            uiList.SelectIndex(descListed.GetIndex(_service, _id));
        }

        private void OnSelectedIndexChanged(int index) {
            if (_desc == null || _service == null || string.IsNullOrEmpty(_id)) { 
                return;
            }
            
            _desc.SetIndex(_service, _id, index);
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