using System;
using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Common.Localization {
    
    public static class LocalizationKeyExtensions {
    
        public static LocalizationKey CreateLocalizationKey(string key, Guid tableGuid) {
            return string.IsNullOrEmpty(key)
                ? default
                : new LocalizationKey(Animator.StringToHash(key), tableGuid);
        }

        public static string GetValue(this LocalizationKey key) {
            return Services.Get<ILocalizationService>().GetLocalizedString(key);
        }
        
        public static T GetValue<T>(this LocalizationKey<T> key) {
            return Services.Get<ILocalizationService>().GetLocalizedAsset<T>(key);
        }
    }
    
}