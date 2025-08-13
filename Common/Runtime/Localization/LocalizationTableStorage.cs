using System;
using UnityEngine;

namespace MisterGames.Common.Localization {

    [CreateAssetMenu(fileName = nameof(LocalizationTableStorage), menuName = "MisterGames/Localization/" + nameof(LocalizationTableStorage))]
    public sealed class LocalizationTableStorage : ScriptableObject, ILocalizationTableStorage {
        
        [LocaleFilter(LocaleFilter.All)]
        [SerializeField] private Locale[] _locales;
        [SerializeField] private ValueRow[] _valueRows;
        
        [Serializable]
        private struct ValueRow {
            public string key;
            public string[] values;
        }

        public int GetLocaleCount() {
            return _locales?.Length ?? 0;
        }

        public void SetLocalesCount(int count) {
            Array.Resize(ref _locales, count);

            for (int i = 0; i < _valueRows?.Length; i++) {
                ref var valueRow = ref _valueRows[i];
                Array.Resize(ref valueRow.values, count);
            }
        }

        public Locale GetLocale(int localeIndex) {
            return _locales[localeIndex];
        }
        
        public void SetLocale(int localeIndex, Locale locale) {
            _locales[localeIndex] = locale;
        }
        
        public void ClearLocales() {
            _locales = Array.Empty<Locale>();
        }
        
        public int GetKeyCount() {
            return _valueRows?.Length ?? 0;
        }
        
        public void SetKeyCount(int count) {
            Array.Resize(ref _valueRows, count);
            
            int localesCount = _locales?.Length ?? 0;
            
            for (int i = 0; i < _valueRows?.Length; i++) {
                ref var valueRow = ref _valueRows[i];
                Array.Resize(ref valueRow.values, localesCount);
            }
        }

        public string GetKey(int keyIndex) {
            ref var valueRow = ref _valueRows[keyIndex];
            return valueRow.key;
        }

        public void SetKey(int keyIndex, string key) {
            ref var valueRow = ref _valueRows[keyIndex];
            valueRow.key = key;
        }
        
        public void ClearKeysAndValues() {
            _valueRows = Array.Empty<ValueRow>();
        }

        public string GetValue(int keyIndex, int localeIndex) {
            ref var valueRow = ref _valueRows[keyIndex];
            return valueRow.values[localeIndex];
        }

        public void SetValue(int keyIndex, int localeIndex, string value) {
            ref var valueRow = ref _valueRows[keyIndex];
            valueRow.values[localeIndex] = value;
        }
    }
    
}