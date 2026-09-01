using System;
using MisterGames.Common.Labels;
using MisterGames.SettingsLib.Descs;
using MisterGames.UI.Components;
using UnityEngine;

namespace MisterGames.SettingsLib.Base {
    
    [Serializable]
    public sealed class SettingBinderFrameRate : ISettingBinder {

        public UiList uiList;
        public LabelValue<ISettingDesc> vsyncSetting;
        
        private ISettingsService _service;
        private ISettingDescListed _desc;
        private ISettingDescListed _vsyncDesc;
        private string _id;
        private bool _ignoreNotify;

        void ISettingBinder.Bind(ISettingsService service, ISettingDesc desc, string id) {
            if (!IsValidSettingDesc(desc, out var descListed)) return;

            _service = service;
            _desc = descListed;
            _id = id;

            _vsyncDesc = vsyncSetting.TryGetData(out var vsyncDesc) && vsyncDesc is VsyncSetting vsync
                ? vsync
                : null;
            
            uiList.OnSelectedIndexChanged += OnSelectedIndexChanged;
            _desc?.AddListener(OnSettingIndexChanged);
            _vsyncDesc?.AddListener(OnVsyncSettingIndexChanged);
        }

        void ISettingBinder.Unbind() {
            uiList.OnSelectedIndexChanged -= OnSelectedIndexChanged;
            _desc?.RemoveListener(OnSettingIndexChanged);
            _vsyncDesc?.RemoveListener(OnVsyncSettingIndexChanged);
            
            uiList.Block(this, false);
            
            _service = null;
            _desc = null;
            _vsyncDesc = null;
            _id = null;
        }

        public void SetupView(ISettingDesc desc) {
            if (!IsValidSettingDesc(desc, out var descListed) || uiList == null) return;

            int count = descListed.GetCount();
            uiList.SetElementsCount(count);
            
            for (int i = 0; i < count; i++) {
                uiList.SetElement(i, descListed.GetValue(i));
            }
            
            uiList.Block(this, block: QualitySettings.vSyncCount > 0);
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
        
        private void OnVsyncSettingIndexChanged(string id, int index) {
            if (_desc == null || _service == null || string.IsNullOrEmpty(_id)) return;
            
            _ignoreNotify = true;
            SetupView(_desc);
            _desc.ApplySetting(_service, _id);
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
            if (desc is not FrameRateSetting d) {
                Debug.LogError($"Setting binder {GetType().Name} requires a {nameof(FrameRateSetting)} setting desc. " +
                               $"Provided invalid desc of type {desc?.GetType().Name}.");
                descListed = null;
                return false;
            }
            descListed = d;
            return true;
        }
    }
    
}