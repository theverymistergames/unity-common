using System;
using System.Collections.Generic;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Common.Localization {

    [CreateAssetMenu(fileName = nameof(LocalizationSettings), menuName = "MisterGames/Localization/" + nameof(LocalizationSettings))]
    public sealed class LocalizationSettings : ScriptableObject {

        [Header("Supported Locales")]
        [LocaleFilter(LocaleFilter.Hardcoded)]
        [SerializeField] private Locale[] _predefinedLocales;
        [SerializeField] private LocaleDescriptor[] _customLocales;

        [Header("Fallback Locales")]
        [SerializeField] private Locale _defaultFallbackLocale;
        [SerializeField] private FallbackGroup[] _fallbackGroups;

        [Header("Tables")]
        [SerializeField] [Min(0)] private float _unloadUnusedTablesDelay = 60f;
        
        [Header("String Values")]
        [SerializeField] private bool _replaceNotLocalizedStringsWithDefaultLocale = true;
        [SerializeField] private bool _replaceEmptyStringsWithFallback = true;
        [VisibleIf(nameof(_replaceEmptyStringsWithFallback))]
        [SerializeField] private string _emptyStringFallback = "<not localized>";
        
        [Header("Asset Values")]
        [SerializeField] private bool _replaceNotLocalizedAssetsWithDefaultLocale = true;

        [Serializable]
        private struct FallbackGroup {
            [LocaleFilter(LocaleFilter.All)]
            public Locale fallbackLocale;
            
            [LocaleFilter(LocaleFilter.All)]
            public Locale[] locales;
        }

        public float UnloadUnusedTablesDelay => _unloadUnusedTablesDelay;
        public bool ReplaceNotLocalizedStringsWithDefaultLocale => _replaceNotLocalizedStringsWithDefaultLocale;
        public bool ReplaceNotLocalizedAssetsWithDefaultLocale => _replaceNotLocalizedAssetsWithDefaultLocale;
        
        private List<Locale> _locales;
        private HashSet<int> _supportedLocales;
        
        private Dictionary<int, Locale> _localeFallbackMap;
        private Dictionary<int, int> _localeHashToIndexMap;

        public string GetFallbackString() {
            return _replaceEmptyStringsWithFallback ? _emptyStringFallback : null;
        }
        
        public bool TryGetSupportedLocale(string localeCode, out Locale locale) {
            localeCode = LocaleExtensions.FormatLocaleCode(localeCode);
            
            if (localeCode == null) {
                locale = default;
                return false;
            }
            
            if (_localeHashToIndexMap == null) FetchLocaleIndices();

            int hash = Animator.StringToHash(localeCode);
            if (_localeHashToIndexMap!.TryGetValue(hash, out int index)) {
                locale = new Locale(hash, index < (_customLocales?.Length ?? 0) ? this : null);
                return true;
            }

            locale = default;
            return false;
        }
        
        public IReadOnlyList<Locale> GetSupportedLocales() {
            if (_locales == null) {
                _locales = new List<Locale>((_predefinedLocales?.Length ?? 0) + (_customLocales?.Length ?? 0));
                
                if (_predefinedLocales != null) _locales.AddRange(_predefinedLocales);
                
                for (int i = 0; i < _customLocales?.Length; i++) {
                    ref var desc = ref _customLocales[i];
                    _locales.Add(new Locale(Animator.StringToHash(desc.code), this));
                }
            }
            
            return _locales;
        }
        
        public Locale GetLocaleOrFallback(Locale locale) {
            if (_localeFallbackMap == null) FetchLocaleMap();

            while (!_supportedLocales.Contains(locale.Hash)) {
                if (!_localeFallbackMap!.TryGetValue(locale.Hash, out locale)) {
                    return _defaultFallbackLocale;
                }
            }

            return locale;
        }
        
        public Locale GetDefaultFallbackLocale() {
            return _defaultFallbackLocale;
        }

        public bool IsSupportedLocale(int localeHash) {
            if (_localeFallbackMap == null) FetchLocaleMap();
            
            return _supportedLocales.Contains(localeHash);
        }
        
        public bool TryGetLocaleDescriptorByHash(int hash, out LocaleDescriptor localeDescriptor) {
            if (_localeHashToIndexMap == null) FetchLocaleIndices();

            if (_localeHashToIndexMap!.TryGetValue(hash, out int index)) {
                if (index < (_customLocales?.Length ?? 0)) {
                    localeDescriptor = _customLocales![index];
                    return true;
                }
                
                return LocaleExtensions.TryGetLocaleDescriptorByHash(hash, out localeDescriptor);
            }
            
            localeDescriptor = default;
            return false;
        }
        
        private void FetchLocaleIndices() {
            int customLength = _customLocales?.Length ?? 0;
            int predefinedLength = _predefinedLocales?.Length ?? 0;
            
            _localeHashToIndexMap ??= new Dictionary<int, int>(customLength + predefinedLength);
            _localeHashToIndexMap.Clear();
            
            for (int i = 0; i < customLength; i++) {
                ref var descriptor = ref _customLocales![i];
                string code = LocaleExtensions.FormatLocaleCode(descriptor.code);
                if (string.IsNullOrEmpty(code)) continue;
                
                _localeHashToIndexMap[Animator.StringToHash(code)] = i;
            }

            for (int i = 0; i < predefinedLength; i++) {
                ref var locale = ref _predefinedLocales![i];
                _localeHashToIndexMap[locale.Hash] = i + customLength;
            }
        }

        private void FetchLocaleMap() {
            _localeFallbackMap ??= new Dictionary<int, Locale>();
            _supportedLocales ??= new HashSet<int>();
            
            var locales = GetSupportedLocales();
            
            for (int i = 0; i < locales.Count; i++) {
                _supportedLocales.Add(locales[i].Hash);
            }

            for (int i = 0; i < _fallbackGroups?.Length; i++) {
                ref var g = ref _fallbackGroups[i];
                
                for (int j = 0; j < g.locales?.Length; j++) {
                    _localeFallbackMap[g.locales[j].Hash] = g.fallbackLocale;
                }
            }
            
            for (int i = 0; i < _fallbackGroups?.Length; i++) {
                ref var g = ref _fallbackGroups[i];
                if (_supportedLocales.Contains(g.fallbackLocale.Hash)) continue;
                
                for (int j = 0; j < g.locales?.Length; j++) {
                    _localeFallbackMap[g.locales[j].Hash] = g.fallbackLocale;
                }
            }
        }
        
#if UNITY_EDITOR
        private void OnValidate() {
            _localeFallbackMap = null;
            _supportedLocales = null;
            _locales = null;
            _localeHashToIndexMap = null;
        }
#endif
    }
    
}