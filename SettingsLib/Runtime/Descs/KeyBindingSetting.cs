using System;
using MisterGames.Common.Localization;
using MisterGames.Input.Actions;
using MisterGames.SettingsLib.Base;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MisterGames.SettingsLib.Descs {
    
    [Serializable]
    public sealed class KeyBindingSetting : ISettingDesc {

        public LocalizationKey name;
        public InputActionRef inputActionRef;
        [Min(0f)] public int bindingIndex;
        
        public void Initialize(ISettingsService service, string id) {
            if (service.TryGet(id, 0, out string controlPath)) {
                inputActionRef.Get().ApplyBindingOverride(bindingIndex, controlPath);
            }
        }
        
        public void Deinitialize(ISettingsService service, string id) {
            
        }

        public LocalizationKey GetName() {
            return name;
        }

        public InputBinding GetBinding() {
            return inputActionRef.Get().bindings[bindingIndex];
        }
        
        public bool SetBinding(ISettingsService service, string id, string controlPath) {
            service.Set(id, 0, controlPath);
            inputActionRef.Get().ApplyBindingOverride(bindingIndex, controlPath);
            return true;
        }
    }
    
}