using System;
using System.Collections.Generic;
using MisterGames.Common.Localization;
using MisterGames.SettingsLib.Base;
using UnityEngine.Device;

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
        }
        
        private readonly HashSet<ISettingDescListed.Listener> _listeners = new();
        private readonly List<Res> _supportedResolutions = new();
        
        public void Initialize(ISettingsService service, string id) {
            for (int i = 0; i < Screen.resolutions.Length; i++) {
                var res = Screen.resolutions[i];
                _supportedResolutions.Add(new Res(res.width, res.height));
            }
        }
        
        public void Deinitialize(ISettingsService service, string id) {
            
        }

        public void ApplySetting(ISettingsService service, string id) {
            if (!service.TryGet(id, 0, out Res res)) {
                res = new Res(Screen.width, Screen.height);
            }

            int index = GetResolutionIndex(res);
            
            NotifyResolution(id, res, index);
        }
        
        public void ClearSetting(ISettingsService service, string id) {
            service.Remove<Res>(id, 0);
        }
        
        public void ResaveSetting(ISettingsService service, string id) {
            if (service.TryGet<Res>(id, 0, out var res)) {
                service.Set(id, 0, res);
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
            var res = service.TryGet(id, index: 0, out Res r) ? r : new Res(Screen.width, Screen.height);
            return GetResolutionIndex(res);
        }

        public bool SetIndex(ISettingsService service, string id, int index) {
            if (index < 0 || index >= _supportedResolutions.Count) return false;

            var res = _supportedResolutions[index];
            bool ok = service.Set(id, index: 0, res);
            
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