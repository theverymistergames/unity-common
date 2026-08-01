using System;
using System.Collections.Generic;
using MisterGames.Common.Lists;
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
        public LocalizationKey vsync;
        public LocalizationKey vsyncHalf;
        public ModeData defaultMode;
        [Min(0)] public int[] fpsNumbers = {
            30,
            60,
            75,
            90,
            120,
            144,
            240,
        };

        [Serializable]
        public struct ModeData {
            public Mode mode;
            public int fps;
            
            public ModeData(int fps, Mode mode) {
                this.fps = fps;
                this.mode = mode;
            }
        }
        
        public enum Mode {
            Unlimited,
            VSync,
            VSyncHalf,
            NumberFps,
        }
        
        private readonly HashSet<ISettingDescListed.Listener> _listeners = new();
        
        public void ApplySetting(ISettingsService service, string id) {
            if (!service.TryGet(id, 0, out ModeData data)) {
                data = defaultMode;
            }

            int index = GetIndex(data);

            NotifyMode(id, data, index);
        }
        
        public void ClearSetting(ISettingsService service, string id) {
            service.Remove<ModeData>(id, 0);
        }
        
        public void ResaveSetting(ISettingsService service, string id) {
            if (service.TryGet<ModeData>(id, 0, out var data)) {
                service.Set(id, 0, data);
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
            return 3 + fpsNumbers.Length;
        }

        public string GetValue(int index) {
            var screenRes = Screen.currentResolution;

            return index switch {
                0 => unlimitedFps.GetValue(),
                1 => string.Format(vsync.GetValue(), string.Format(numberFps.GetValue(), Mathf.RoundToInt((float) screenRes.refreshRateRatio.value))),
                2 => string.Format(vsyncHalf.GetValue(), string.Format(numberFps.GetValue(), Mathf.RoundToInt((float) screenRes.refreshRateRatio.value * 0.5f))),
                _ => string.Format(numberFps.GetValue(), fpsNumbers[index - 3])
            };
        }

        public int GetIndex(ISettingsService service, string id) {
            var data = service.TryGet(id, index: 0, out ModeData m) ? m : defaultMode;
            return GetIndex(data);
        }

        public bool SetIndex(ISettingsService service, string id, int index) {
            if (index < 0 || index >= fpsNumbers.Length + 3) return false;

            var mode = index switch {
                0 => new ModeData(0, Mode.Unlimited),
                1 => new ModeData(0, Mode.VSync),
                2 => new ModeData(0, Mode.VSyncHalf),
                _ => new ModeData(fpsNumbers[index - 3], Mode.NumberFps)
            };
            
            bool ok = service.Set(id, index: 0, mode);
            
            NotifyMode(id, mode, index);
            
            return ok;
        }
        
        private int GetIndex(ModeData data) {
            return data.mode switch {
                Mode.Unlimited => 0,
                Mode.VSync => 1,
                Mode.VSyncHalf => 2,
                Mode.NumberFps => 3 + fpsNumbers.TryFindIndex(data.fps, (x, y) => x == y),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private void NotifyMode(string id, ModeData data, int index) {
            switch (data.mode) {
                case Mode.Unlimited:
                    QualitySettings.vSyncCount = 0;
                    Application.targetFrameRate = -1;
                    break;
                
                case Mode.VSync:
                    QualitySettings.vSyncCount = 1;
                    break;
                
                case Mode.VSyncHalf:
                    QualitySettings.vSyncCount = 2;
                    break;
                
                case Mode.NumberFps:
                    QualitySettings.vSyncCount = 0;
                    Application.targetFrameRate = data.fps;
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            foreach (var listener in _listeners) {
                listener.Invoke(id, index);
            }
        }
    }
    
}