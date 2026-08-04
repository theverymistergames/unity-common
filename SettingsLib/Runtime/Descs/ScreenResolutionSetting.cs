using System;
using System.Collections.Generic;
using MisterGames.Common.Localization;
using MisterGames.Common.Maths;
using MisterGames.SettingsLib.Base;
using Unity.Collections;
using Screen = UnityEngine.Device.Screen;

namespace MisterGames.SettingsLib.Descs {
    
    [Serializable]
    public sealed class ScreenResolutionSetting : ISettingDescListed {

        public LocalizationKey name;
        
        [Serializable]
        private struct Res {
            public int width;
            public int height;

            public Res(int width, int height) {
                this.width = width;
                this.height = height;
            }

            public static Res FromLong(long value) {
                NumberExtensions.LongAsTwoInts(value, out int x, out int y);
                return new Res(x, y);
            }

            public long ToLong() {
                return NumberExtensions.TwoIntsAsLong(width, height);
            }

            public override string ToString() {
                return $"[{width}x{height}]";
            }
        }
        
        private readonly HashSet<ISettingDescListed.Listener> _listeners = new();
        private readonly List<Res> _supportedResolutions = new();
        
        public void Initialize(ISettingsService service, string id) {
            var set = new NativeHashSet<long>(Screen.resolutions.Length, Allocator.Temp);
            
            for (int i = 0; i < Screen.resolutions.Length; i++) {
                var r = Screen.resolutions[i];
                var res = new Res(r.width, r.height);
                
                if (!set.Add(res.ToLong())) continue;
                
                _supportedResolutions.Add(res);
            }

            set.Dispose();
        }
        
        public void Deinitialize(ISettingsService service, string id) {
            
        }

        public void ApplySetting(ISettingsService service, string id) {
            var res = new Res(Screen.width, Screen.height);
            
            if (service.TryGet(id, 0, out long value)) {
                res = Res.FromLong(value);
            }

            int index = GetResolutionIndex(res);

            NotifyResolution(id, res, index);
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
            return _supportedResolutions.Count;
        }

        public string GetValue(int index) {
            var res = _supportedResolutions[index];
            return $"{res.width}x{res.height}";
        }

        public int GetIndex(ISettingsService service, string id) {
            var res = new Res(Screen.width, Screen.height);
            
            if (service.TryGet(id, 0, out long value)) {
                res = Res.FromLong(value);
            }
            
            return GetResolutionIndex(res);
        }

        public bool SetIndex(ISettingsService service, string id, int index) {
            if (index < 0 || index >= _supportedResolutions.Count) return false;
            
            var res = _supportedResolutions[index];
            bool ok = service.Set(id, index: 0, res.ToLong());

            NotifyResolution(id, res, index);
            
            return ok;
        }

        private void NotifyResolution(string id, Res res, int index) {
            Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);
            
            foreach (var listener in _listeners) {
                listener.Invoke(id, index);
            }
        }
        
        private int GetResolutionIndex(Res res) {
            for (int i = 0; i < _supportedResolutions.Count; i++) {
                var r = _supportedResolutions[i];
                if (r.width != res.width || r.height != res.height) continue;
                
                return i;
            }
            
            return -1;
        }
    }
    
}