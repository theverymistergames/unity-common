using System;
using System.Collections.Generic;
using MisterGames.Common.Attributes;
using MisterGames.Common.Easing;
using MisterGames.Common.Localization;
using MisterGames.SettingsLib.Base;
using UnityEngine;

namespace MisterGames.SettingsLib.Descs {
    
    [Serializable]
    public sealed class NormalizedFloatSetting : ISettingDescValued<float> {

        public LocalizationKey name;
        public float defaultNormalizedValue = 1f;
        public Vector2 remap;
        public AnimationCurve curve = EasingType.Linear.ToAnimationCurve();
        [SerializeReference] [SubclassSelector] public ISettingReaderFloat reader;
        
        private readonly HashSet<ISettingDescValued<float>.Listener> _listeners = new();
        
        public LocalizationKey GetName() {
            return name;
        }

        public void AddListener(ISettingDescValued<float>.Listener listener) {
            _listeners.Add(listener);
        }
        
        public void RemoveListener(ISettingDescValued<float>.Listener listener) {
            _listeners.Remove(listener);
        }

        public void ApplySetting(ISettingsService service, string id) {
            NotifyValue(id, GetValue(service, id));
        }

        public void ClearSetting(ISettingsService service, string id) {
            service.Remove<float>(id, 0);
        }

        public void ResaveSetting(ISettingsService service, string id) {
            if (service.TryGet(id, 0, out float value)) {
                service.Set(id, 0, value);
            }
        }

        public float GetDefaultValue() {
            return defaultNormalizedValue;
        }

        public float GetValue(ISettingsService service, string id) {
            return service.TryGet(id, 0,  out float val) ? val : defaultNormalizedValue;
        }
        
        public void SetValue(ISettingsService service, string id, float value) {
            service.Set(id, 0, value);
            NotifyValue(id, value);
        }

        private void NotifyValue(string id, float value) {
            reader?.OnReadValue(GetRealValue(value));
            
            foreach (var listener in _listeners) { 
                listener.Invoke(id, value);    
            }
        }
        
        private float GetRealValue(float normalizedValue) {
            return Mathf.Lerp(remap.x, remap.y, curve.Evaluate(normalizedValue));
        }
    }
    
}