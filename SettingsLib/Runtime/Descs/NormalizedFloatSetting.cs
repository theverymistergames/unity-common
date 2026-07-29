using System;
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
        
        public LocalizationKey GetName() {
            return name;
        }

        public void Initialize(ISettingsService service, string id) {
            reader?.OnReadValue(GetRealValue(GetValue(service, id)));
        }
        
        public void Deinitialize(ISettingsService service, string id) {
            
        }

        public float GetDefaultValue() {
            return defaultNormalizedValue;
        }

        public float GetValue(ISettingsService service, string id) {
            return service.TryGet(id, 0,  out float val) ? val : defaultNormalizedValue;
        }
        
        public void SetValue(ISettingsService service, string id, float value) {
            reader?.OnReadValue(GetRealValue(value));
            service.Set(id, 0, value);
        }

        private float GetRealValue(float normalizedValue) {
            return Mathf.Lerp(remap.x, remap.y, curve.Evaluate(normalizedValue));
        }
    }
    
}