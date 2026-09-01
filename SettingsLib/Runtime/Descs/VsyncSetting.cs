using System;
using System.Collections.Generic;
using MisterGames.Common.Localization;
using MisterGames.SettingsLib.Base;
using UnityEngine;

namespace MisterGames.SettingsLib.Descs {
    
    [Serializable]
    public sealed class VsyncSetting : ISettingDescListed {

        public LocalizationKey name;
        public LocalizationKey vsyncOff;
        public LocalizationKey vsync;
        public LocalizationKey vsyncHalf;
        public Mode defaultMode = Mode.VSync;
        
        public enum Mode {
            VsyncOff,
            VSync,
            VSyncHalf,
        }
        
        private readonly HashSet<ISettingDescListed.Listener> _listeners = new();
        
        public void ApplySetting(ISettingsService service, string id) {
            var data = defaultMode;
            
            if (service.TryGet(id, 0, out int value)) {
                data = (Mode) value;
            }

            int index = GetIndex(data);

            NotifyMode(id, data, index);
        }
        
        public void ClearSetting(ISettingsService service, string id) {
            service.Remove<int>(id, 0);
        }
        
        public void ResaveSetting(ISettingsService service, string id) {
            if (service.TryGet(id, 0, out int value)) {
                service.Set(id, 0, value);
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
            return 3;
        }

        public string GetValue(int index) {
            return index switch {
                0 => string.Format(vsyncOff.GetValue()),
                1 => string.Format(vsync.GetValue()),
                _ => string.Format(vsyncHalf.GetValue())
            };
        }

        public int GetIndex(ISettingsService service, string id) {
            var data = defaultMode;
            
            if (service.TryGet(id, 0, out int value)) {
                data = (Mode) value;
            }

            return GetIndex(data);
        }

        public bool SetIndex(ISettingsService service, string id, int index) {
            if (index is < 0 or >= 3) return false;

            bool ok = service.Set(id, index: 0, index);
            
            NotifyMode(id, (Mode) index, index);
            
            return ok;
        }
        
        private int GetIndex(Mode mode) {
            return (int) mode;
        }

        private void NotifyMode(string id, Mode mode, int index) {
            QualitySettings.vSyncCount = mode switch {
                Mode.VsyncOff => 0,
                Mode.VSync => 1,
                Mode.VSyncHalf => 2,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            foreach (var listener in _listeners) {
                listener.Invoke(id, index);
            }
        }
    }
    
}