using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Common.Inputs;
using MisterGames.Common.Lists;
using MisterGames.Common.Localization;
using MisterGames.Common.Service;
using MisterGames.SettingsLib.Base;
using UnityEngine;

namespace MisterGames.SettingsLib.Descs {
    
    [Serializable]
    public sealed class GraphicsTierSetting : ISettingDescListed {

        public LocalizationKey name;
        public SerializedDictionary<string, LocalizationKey> tiers;
        public string defaultTier = "High Fidelity";
        public DeviceOption[] deviceOptions;
        
        [Serializable]
        public struct DeviceOption {
            public string defaultTier;
            public InputDeviceType[] deviceTypes;
            public GamepadType[] gamepadTypes;
        }
        
        private readonly HashSet<ISettingDescListed.Listener> _listeners = new();
        
        public void ApplySetting(ISettingsService service, string id) {
            if (!service.TryGet(id, 0, out string tier)) {
                tier = GetDefaultTier();
            }

            NotifyMode(id, tier);
        }
        
        public void ClearSetting(ISettingsService service, string id) {
            service.Remove<string>(id, 0);
        }
        
        public void ResaveSetting(ISettingsService service, string id) {
            if (service.TryGet(id, 0, out string tier)) {
                service.Set(id, 0, tier);
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
            return tiers.Count;
        }

        public string GetValue(int index) {
            return tiers.GetEntry(index).value.GetValue();
        }

        public int GetIndex(ISettingsService service, string id) {
            string tier = service.TryGet(id, index: 0, out string t) ? t : GetDefaultTier();
            return tiers.FirstIndexOf(tier, (x, e) => x == e.key);
        }

        public bool SetIndex(ISettingsService service, string id, int index) {
            if (index < 0 || index >= tiers.Count) return false;

            string tier = tiers.GetEntry(index).key;
            bool ok = service.Set(id, index: 0, tier);
            
            NotifyMode(id, tier);
            
            return ok;
        }

        private void NotifyMode(string id, string tier) {
            int elementIndex = tiers.FirstIndexOf(tier, (x, e) => x == e.key);
            int tierIndex = Array.IndexOf(QualitySettings.names, tier);
            
            foreach (var listener in _listeners) {
                listener.Invoke(id, elementIndex);
            }
            
            QualitySettings.SetQualityLevel(tierIndex, applyExpensiveChanges: true);
        }

        private string GetDefaultTier() {
            if (!Services.TryGet(out IDeviceService deviceService)) {
                return defaultTier;
            }

            var deviceType = deviceService.CurrentDevice;
            var gamepadType = deviceService.GamepadType;
            
            for (int i = 0; i < deviceOptions.Length; i++) {
                ref var opt = ref deviceOptions[i];

                if (string.IsNullOrWhiteSpace(opt.defaultTier) ||
                    !opt.deviceTypes.Contains(deviceType) || 
                    opt.gamepadTypes is { Length: > 0 } && !opt.gamepadTypes.Contains(gamepadType)) 
                {
                    continue;
                }
                
                return opt.defaultTier;
            }
            
            return defaultTier;
        }
    }
    
}