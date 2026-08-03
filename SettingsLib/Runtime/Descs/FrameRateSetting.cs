using System;
using System.Collections.Generic;
using MisterGames.Common.Localization;
using MisterGames.Common.Maths;
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
            [Min(0)] public int fpsIndex;
            
            public ModeData(Mode mode, int fpsIndex) {
                this.fpsIndex = fpsIndex;
                this.mode = mode;
            }

            public long AsLong() {
                return NumberExtensions.TwoIntsAsLong((int) mode, fpsIndex);
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
            var data = defaultMode;
            
            if (service.TryGet(id, 0, out long value)) {
                NumberExtensions.LongAsTwoInts(value, out int a, out int b);
                if (a is >= 0 and <= 3 && b >= 0 && b < fpsNumbers.Length) {
                    data = new ModeData((Mode) a, b);
                }
            }

            int index = GetIndex(data);

            NotifyMode(id, data, index);
        }
        
        public void ClearSetting(ISettingsService service, string id) {
            service.Remove<long>(id, 0);
        }
        
        public void ResaveSetting(ISettingsService service, string id) {
            if (service.TryGet(id, 0, out long value)) {
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
            return 3 + fpsNumbers.Length;
        }

        public string GetValue(int index) {
            var screenRes = Screen.currentResolution;

            return index switch {
                0 => unlimitedFps.GetValue(),
                1 => string.Format(vsync.GetValue(), string.Format(numberFps.GetValue(), Mathf.RoundToInt((float) screenRes.refreshRateRatio.value))),
                2 => string.Format(vsyncHalf.GetValue(), string.Format(numberFps.GetValue(), Mathf.RoundToInt((float) screenRes.refreshRateRatio.value))),
                _ => string.Format(numberFps.GetValue(), index - 3 >= 0 && index - 3 < fpsNumbers.Length ? fpsNumbers[index - 3] : fpsNumbers[0])
            };
        }

        public int GetIndex(ISettingsService service, string id) {
            var data = defaultMode;
            
            if (service.TryGet(id, 0, out long value)) {
                NumberExtensions.LongAsTwoInts(value, out int a, out int b);
                if (a is >= 0 and <= 3 && b >= 0 && b < fpsNumbers.Length) {
                    data = new ModeData((Mode) a, b);
                }
            }

            return GetIndex(data);
        }

        public bool SetIndex(ISettingsService service, string id, int index) {
            if (index < 0 || index >= fpsNumbers.Length + 3) return false;

            var mode = index switch {
                0 => new ModeData(Mode.Unlimited, 0),
                1 => new ModeData(Mode.VSync, 0),
                2 => new ModeData(Mode.VSyncHalf, 0),
                _ => new ModeData(Mode.NumberFps, index - 3)
            };
            
            bool ok = service.Set(id, index: 0, mode.AsLong());
            
            NotifyMode(id, mode, index);
            
            return ok;
        }
        
        private int GetIndex(ModeData data) {
            return data.mode switch {
                Mode.Unlimited => 0,
                Mode.VSync => 1,
                Mode.VSyncHalf => 2,
                Mode.NumberFps => 3 + Mathf.Clamp(data.fpsIndex, 0, fpsNumbers.Length),
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
                    Application.targetFrameRate = fpsNumbers[data.fpsIndex >= 0 && data.fpsIndex < fpsNumbers.Length ? data.fpsIndex : 0];
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