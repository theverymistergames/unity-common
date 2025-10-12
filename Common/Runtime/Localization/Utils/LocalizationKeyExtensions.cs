using System;
using System.Runtime.CompilerServices;
using MisterGames.Common.Data;
using MisterGames.Common.Service;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Common.Localization {
    
    public static class LocalizationKeyExtensions {
    
        public static LocalizationKey CreateLocalizationKey(string key, Guid tableGuid) {
            return string.IsNullOrEmpty(key)
                ? default
                : new LocalizationKey(Animator.StringToHash(key), tableGuid);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull(this LocalizationKey key) => key.table == SerializedGuid.Empty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull<T>(this LocalizationKey<T> key) => key.table == SerializedGuid.Empty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNull(this LocalizationKey key) => key.table != SerializedGuid.Empty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNull<T>(this LocalizationKey<T> key) => key.table != SerializedGuid.Empty;

        public static string GetValue(this LocalizationKey key) {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return LoadTableStorageAssetInEditor(key, out int index) is ILocalizationTableStorage<string> table &&
                       table.TryGetValue(index, 0, out string value)
                    ? value
                    : null;
            }
#endif
            
            return Services.Get<ILocalizationService>().GetLocalizedString(key);
        }
        
        public static T GetValue<T>(this LocalizationKey<T> key) {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return LoadTableStorageAssetInEditor(new LocalizationKey(key.hash, key.table.ToGuid()), out int index) is ILocalizationTableStorage<T> table &&
                       table.TryGetValue(index, 0, out var value)
                    ? value
                    : default;
            }
#endif
            
            return Services.Get<ILocalizationService>().GetLocalizedAsset(key);
        }
        
        public static string GetValue(this LocalizationKey key, Locale locale) {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return LoadTableStorageAssetInEditor(key, out int index) is ILocalizationTableStorage<string> table &&
                       table.TryGetValue(index, GetLocaleIndex(table, locale), out string value)
                    ? value
                    : null;
            }
#endif
            
            return Services.Get<ILocalizationService>().GetLocalizedString(key, locale);
        }
        
        public static T GetValue<T>(this LocalizationKey<T> key, Locale locale) {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return LoadTableStorageAssetInEditor(new LocalizationKey(key.hash, key.table.ToGuid()), out int index) is ILocalizationTableStorage<T> table &&
                       table.TryGetValue(index, GetLocaleIndex(table, locale), out var value)
                    ? value
                    : default;
            }
#endif
            
            return Services.Get<ILocalizationService>().GetLocalizedAsset(key, locale);
        }

        private static ILocalizationTableStorage LoadTableStorageAssetInEditor(LocalizationKey key, out int keyIndex) {
            keyIndex = -1;
            
            string path = AssetDatabase.GUIDToAssetPath(key.table.ToGuid().ToUnityEditorGUID());
            var table = AssetDatabase.LoadAssetAtPath<LocalizationTableStorageBase>(path);
            
            if (table == null) return null;

            int count = table.GetKeyCount();
            for (int i = 0; i < count; i++) {
                string value = table.GetKey(i);
                int hash = string.IsNullOrWhiteSpace(value) ? 0 : Animator.StringToHash(value);
                
                if (key.hash != hash) continue;
                
                keyIndex = i;
                return table;
            }
            
            return table;
        }

        private static int GetLocaleIndex(ILocalizationTableStorage table, Locale locale) {
            if (table == null || locale.IsNull()) return 0;

            int count = table.GetLocaleCount();
            for (int i = 0; i < count; i++) {
                if (table.GetLocale(i) == locale) return i;
            }
            
            return 0;
        }
    }
    
}