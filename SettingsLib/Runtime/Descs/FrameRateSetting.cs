using System;
using System.Collections.Generic;
using MisterGames.Common.Localization;
using MisterGames.SettingsLib.Base;
using UnityEngine;
using Screen = UnityEngine.Device.Screen;

namespace MisterGames.SettingsLib.Descs {
    
    [Serializable]
    public sealed class FrameRateSetting : ISettingDescListed {

        public LocalizationKey name;
        public LocalizationKey unlimitedFps;
        public LocalizationKey numberFps;
        [Min(0)] public int defaultMode;
        [Min(0)] public int[] fpsNumbers = {
            60,
            75,
            90,
            120,
            144,
            240,
            -1
        };

        private readonly HashSet<ISettingDescListed.Listener> _listeners = new();
        
        public void ApplySetting(ISettingsService service, string id) {
            int data = defaultMode;
            
            if (service.TryGet(id, 0, out int value)) {
                data = value;
            }

            int index = GetIndex(data);

            NotifyMode(id, index);
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
            return fpsNumbers.Length;
        }

        public string GetValue(int index) {
            if (QualitySettings.vSyncCount != 0) {
                float refreshRate = (float) Screen.currentResolution.refreshRateRatio.value;
                return string.Format(numberFps.GetValue(), Mathf.RoundToInt(refreshRate / QualitySettings.vSyncCount));
            }

            if (index < 0 || index >= fpsNumbers.Length || fpsNumbers[index] < 0) {
                return unlimitedFps.GetValue();
            }

            return string.Format(numberFps.GetValue(), fpsNumbers[index]);
        }

        public int GetIndex(ISettingsService service, string id) {
            int data = defaultMode;
            
            if (service.TryGet(id, 0, out int value)) {
                data = value;
            }

            return GetIndex(data);
        }

        public bool SetIndex(ISettingsService service, string id, int index) {
            if (index < 0 || index >= fpsNumbers.Length) return false;
            
            bool ok = service.Set(id, index: 0, index);
            
            NotifyMode(id, index);
            
            return ok;
        }
        
        private int GetIndex(int index) {
            return index < 0 || index >= fpsNumbers.Length ? -1 : index;
        }

        private void NotifyMode(string id, int index) {
            int fps = fpsNumbers[index];
            Application.targetFrameRate = fps < 0 ? -1 : fps;

            foreach (var listener in _listeners) {
                listener.Invoke(id, index);
            }
        }
    }
    
}