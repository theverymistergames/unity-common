using System;
using System.Collections.Generic;
using MisterGames.Common.Attributes;
using MisterGames.Common.Easing;
using MisterGames.Common.Localization;
using MisterGames.SettingsLib.Base;
using UnityEngine;

namespace MisterGames.SettingsLib.Descs {
    
    [Serializable]
    public sealed class BoolSetting : ISettingDescValued<bool> {

        public LocalizationKey name;
        public bool defaultValue;
        [SerializeReference] [SubclassSelector] public ISettingReaderBool reader;
        
        private readonly HashSet<ISettingDescValued<bool>.Listener> _listeners = new();
        
        public LocalizationKey GetName() {
            return name;
        }

        public void AddListener(ISettingDescValued<bool>.Listener listener) {
            _listeners.Add(listener);
        }
        
        public void RemoveListener(ISettingDescValued<bool>.Listener listener) {
            _listeners.Remove(listener);
        }

        public void ApplySetting(ISettingsService service, string id) {
            NotifyValue(id, GetValue(service, id));
        }

        public void ClearSetting(ISettingsService service, string id) {
            service.Remove<bool>(id, 0);
        }

        public void ResaveSetting(ISettingsService service, string id) {
            if (service.TryGet(id, 0, out float value)) {
                service.Set(id, 0, value);
            }
        }

        public bool GetDefaultValue() {
            return defaultValue;
        }

        public bool GetValue(ISettingsService service, string id) {
            return service.TryGet(id, 0,  out bool val) ? val : defaultValue;
        }
        
        public void SetValue(ISettingsService service, string id, bool value) {
            service.Set(id, 0, value);
            NotifyValue(id, value);
        }

        private void NotifyValue(string id, bool value) {
            reader?.OnReadValue(value);
            
            foreach (var listener in _listeners) { 
                listener.Invoke(id, value);    
            }
        }
    }
    
}