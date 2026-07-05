using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Service;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Common.Localization {

    internal static class LocalizationSettingsCache {

        private static IReadOnlyList<LocalizationSettings> _localizationSettingsList;
        
        public static IReadOnlyList<LocalizationSettings> GetAllLocalizationSettings() {
            return _localizationSettingsList ??= FindAllLocalizationSettings();
        }

        private static IReadOnlyList<LocalizationSettings> FindAllLocalizationSettings() {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return AssetDatabase
                    .FindAssets($"a:assets t:{nameof(LocalizationSettings)}")
                    .Select(guid => AssetDatabase.LoadAssetAtPath<LocalizationSettings>(AssetDatabase.GUIDToAssetPath(guid)))
                    .ToArray();
#endif
            
            return new[] { Services.Get<ILocalizationService>().Settings };
        }
    }
    
}