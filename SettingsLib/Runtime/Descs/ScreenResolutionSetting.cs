using System;
using System.Collections.Generic;
using MisterGames.Common.Localization;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using MisterGames.SettingsLib.Base;
using Unity.Collections;
using UnityEngine;
using Screen = UnityEngine.Device.Screen;

namespace MisterGames.SettingsLib.Descs {

    [Serializable]
    public sealed class ScreenResolutionSetting : ISettingDescListed, IUpdate {

        public LocalizationKey name;

        [Serializable]
        private struct Res {
            public int width;
            public int height;

            public Res(Resolution r) {
                width = r.width;
                height = r.height;
            }

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

        private ISettingsService _service;
        private string _id;
        private string _displayName;
        private Vector2Int _displayPosition;

        public void Initialize(ISettingsService service, string id) {
            _service = service;
            _id = id;

            FetchSupportedResolutions();
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        public void Deinitialize(ISettingsService service, string id) {
            PlayerLoopStage.LateUpdate.Unsubscribe(this);

            _service = null;
            _id = null;
        }

        void IUpdate.OnUpdate(float dt) {
            var displayInfo = Screen.mainWindowDisplayInfo;

            var displayPosition = displayInfo.workArea.position;

            if (displayPosition == _displayPosition && string.Equals(displayInfo.name, _displayName, StringComparison.Ordinal)) {
                return;
            }

            FetchSupportedResolutions();

            if (_service != null && !string.IsNullOrEmpty(_id)) ApplySetting(_service, _id);
        }

        public void ApplySetting(ISettingsService service, string id) {
            var res = GetTargetResolution(service, id);
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
            return GetResolutionIndex(GetTargetResolution(service, id));
        }

        public bool SetIndex(ISettingsService service, string id, int index) {
            if (index < 0 || index >= _supportedResolutions.Count) return false;

            var res = _supportedResolutions[index];
            bool ok = service.Set(id, index: 0, res.ToLong());

            NotifyResolution(id, res, index);

            return ok;
        }

        private void FetchSupportedResolutions() {
            var displayInfo = Screen.mainWindowDisplayInfo;
            var resolutions = Screen.resolutions;

            _displayName = displayInfo.name;
            _displayPosition = displayInfo.workArea.position;
            _supportedResolutions.Clear();

            var set = new NativeHashSet<long>(resolutions.Length + 1, Allocator.Temp);

            for (int i = 0; i < resolutions.Length; i++) {
                var res = new Res(resolutions[i]);
                if (!set.Add(res.ToLong())) continue;

                _supportedResolutions.Add(res);
            }

            var nativeRes = GetNativeResolution();
            bool addedNativeRes = set.Add(nativeRes.ToLong());
            if (addedNativeRes) _supportedResolutions.Add(nativeRes);

            set.Dispose();
        }

        private Res GetTargetResolution(ISettingsService service, string id) {
            var nativeRes = GetNativeResolution();

            if (!service.TryGet(id, 0, out long value)) return nativeRes;

            var savedRes = Res.FromLong(value);
            if (GetResolutionIndex(savedRes) >= 0) return savedRes;

            return nativeRes;
        }

        private static Res GetNativeResolution() {
            var displayInfo = Screen.mainWindowDisplayInfo;
            var nativeRes = new Res(displayInfo.width, displayInfo.height);
            var resolutions = Screen.resolutions;

            for (int i = 0; i < resolutions.Length; i++) {
                var res = new Res(resolutions[i]);
                if ((long) res.width * res.height > (long) nativeRes.width * nativeRes.height) nativeRes = res;
            }

            return nativeRes;
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
