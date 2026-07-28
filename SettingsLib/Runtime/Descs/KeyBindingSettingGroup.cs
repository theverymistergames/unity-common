using System;
using System.Collections.Generic;
using MisterGames.Common.Labels;
using MisterGames.Common.Localization;
using MisterGames.Common.Service;
using MisterGames.SettingsLib.Base;
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
                if (label.GetData() is not KeyBindingSetting setting) continue;
                
                _keyBindingSettings[label.GetFullLabel()] = setting;
                setting.AddBindingListener(NotifyBindingApplied);
            }
        }
        
        public void Deinitialize(ISettingsService service, string id) {
            foreach (var setting in _keyBindingSettings.Values) {
                setting.RemoveBindingListener(NotifyBindingApplied);
            }
        }

        public LocalizationKey GetName() {
            return default;
        }

        private void NotifyBindingApplied(string id, InputAction action, int bindingIndex, string path) {
            if (_suppressNotify || !Services.TryGet(out ISettingsService service)) return;
            
            _suppressNotify = true;
            
            foreach ((string label, var setting) in _keyBindingSettings) {
                if (!setting.TryGetBinding(out var inputAction, out var binding, out int index) ||
                    action == inputAction && bindingIndex == index || 
                    inputAction.bindings[index].effectivePath != path) 
                {
                    continue;
                }
                
                setting.ClearBinding(service, label);
            }

            _suppressNotify = false;
        }
    }
    
}