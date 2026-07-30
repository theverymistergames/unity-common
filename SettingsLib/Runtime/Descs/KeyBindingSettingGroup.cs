using System;
using System.Collections.Generic;
using MisterGames.Common.Labels;
using MisterGames.Common.Localization;
using MisterGames.Common.Service;
using MisterGames.SettingsLib.Base;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MisterGames.SettingsLib.Descs {
    
    [Serializable]
    public sealed class KeyBindingSettingGroup : ISettingDesc {

        public LabelValue<ISettingDesc>[] keyBindingSettings;
        
        private readonly Dictionary<string, KeyBindingSetting> _keyBindingSettings = new();
        private bool _suppressNotify;

        public void Initialize(ISettingsService service, string id) {
            for (int i = 0; i < keyBindingSettings.Length; i++) {
                var label = keyBindingSettings[i];
                if (label.GetData() is not KeyBindingSetting setting) {
                    Debug.LogError($"{nameof(KeyBindingSettingGroup)} [{id}]: setting #{i} [{label}] is not a {nameof(KeyBindingSetting)}. " +
                                   $"Setting of type {nameof(KeyBindingSetting)} is required. Skipping this setting.");
                    continue;
                }
                
                _keyBindingSettings[label.GetFullLabel()] = setting;
                setting.AddBindingListener(NotifyBindingApplied);
                setting.AddToGroup(this);
            }
        }
        
        public void Deinitialize(ISettingsService service, string id) {
            foreach (var setting in _keyBindingSettings.Values) {
                setting.RemoveBindingListener(NotifyBindingApplied);
                setting.RemoveFromGroup(this);
            }
        }

        public void ApplySetting(ISettingsService service, string id) {
            
        }

        public void ClearSetting(ISettingsService service, string id) {
            
        }

        public void ResaveSetting(ISettingsService service, string id) {
            
        }

        public LocalizationKey GetName() {
            return default;
        }

        public Dictionary<string, KeyBindingSetting> GetKeyBindings() {
            return _keyBindingSettings;
        }

        private void NotifyBindingApplied(string id, InputAction action, int bindingIndex, string path) {
            if (_suppressNotify || !Services.TryGet(out ISettingsService service)) return;
            
            _suppressNotify = true;
            
            foreach ((string label, var setting) in _keyBindingSettings) {
                if (id == label ||
                    !setting.TryGetBinding(out _, out var binding, out int _) ||
                    binding.effectivePath != path) 
                {
                    continue;
                }
                
                setting.ClearBinding(service, label);
            }

            _suppressNotify = false;
        }
    }
    
}