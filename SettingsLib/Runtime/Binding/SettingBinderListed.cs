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
        private bool _ignoreNotify;

        void ISettingBinder.Bind(ISettingsService service, ISettingDesc desc, string id) {
            if (!IsValidSettingDesc(desc, out var descListed)) return;

            _service = service;
            _desc = descListed;
            _id = id;
            
            uiList.OnSelectedIndexChanged += OnSelectedIndexChanged;
            _desc?.AddListener(OnSettingIndexChanged);
        }

        void ISettingBinder.Unbind() {
            uiList.OnSelectedIndexChanged -= OnSelectedIndexChanged;
            _desc?.RemoveListener(OnSettingIndexChanged);
            
            _service = null;
            _desc = null;
            _id = null;
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
            if (_ignoreNotify || !IsValidSettingDesc(desc, out var descListed) || _service == null || string.IsNullOrEmpty(_id)) {
                return;
            }

            _ignoreNotify = true;
            uiList.SelectIndex(descListed.GetIndex(_service, _id));
            _ignoreNotify = false;
        }

        private void OnSettingIndexChanged(string id, int index) {
            if (_ignoreNotify) return;
            
            _ignoreNotify = true;
            uiList.SelectIndex(index);
            _ignoreNotify = false;
        }

        private void OnSelectedIndexChanged(int index) {
            if (_ignoreNotify || _desc == null || _service == null || string.IsNullOrEmpty(_id)) { 
                return;
            }
            
            _ignoreNotify = true;
            _desc.SetIndex(_service, _id, index);
            _ignoreNotify = false;
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