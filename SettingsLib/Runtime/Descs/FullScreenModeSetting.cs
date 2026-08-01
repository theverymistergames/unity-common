using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Common.Localization;
using MisterGames.SettingsLib.Base;
using UnityEngine;
using Screen = UnityEngine.Device.Screen;

namespace MisterGames.SettingsLib.Descs {
    
    [Serializable]
    public sealed class FullScreenModeSetting : ISettingDescListed {

        public LocalizationKey name;
        public SerializedDictionary<FullScreenMode, LocalizationKey> modes;
        public FullScreenMode defaultMode;
        
        private readonly HashSet<ISettingDescListed.Listener> _listeners = new();
        
        public void ApplySetting(ISettingsService service, string id) {
            if (!service.TryGet(id, 0, out FullScreenMode mode)) {
                mode = defaultMode;
            }

            int index = modes.FirstIndexOf(mode, (x, m) => x == m.key);

            NotifyMode(id, mode, index);
        }
        
        public void ClearSetting(ISettingsService service, string id) {
            service.Remove<FullScreenMode>(id, 0);
        }
        
        public void ResaveSetting(ISettingsService service, string id) {
            if (service.TryGet<FullScreenMode>(id, 0, out var mode)) {
                service.Set(id, 0, mode);
            }
        }
        
        public LocalizationKey GetName() {
            return name;
        }

        public void AddListener(ISettingDescListed.Listener listener) {
            _listeners.Add(listener);
        }
        
        public void RemoveListener(ISettingDescListed.Listener listener) {
            _listeners.Remove(listener);
        }

        public int GetCount() {
            return modes.Count;
        }

        public string GetValue(int index) {
            return modes.GetEntry(index).value.GetValue();
        }

        public int GetIndex(ISettingsService service, string id) {
            var mode = service.TryGet(id, index: 0, out FullScreenMode m) ? m : defaultMode;
            return modes.FirstIndexOf(mode, (x, e) => x == e.key);
        }

        public bool SetIndex(ISettingsService service, string id, int index) {
            if (index < 0 || index >= modes.Count) return false;

            var mode = modes.GetEntry(index).key;
            bool ok = service.Set(id, index: 0, mode);
            
            NotifyMode(id, mode, index);
            
            return ok;
        }

        private void NotifyMode(string id, FullScreenMode mode, int index) {
            Screen.fullScreenMode = mode;

            foreach (var listener in _listeners) {
                listener.Invoke(id, index);
            }
        }
    }
    
}