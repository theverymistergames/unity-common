using System;
using System.Collections.Generic;
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

        public Locale GetLocale(int localeIndex) {
            return _locales[localeIndex];
        }
        
        public int GetKeyCount() {
            return _valueRows?.Length ?? 0;
        }

        public string GetKey(int keyIndex) {
            ref var valueRow = ref _valueRows[keyIndex];
            return valueRow.key;
        }

        public string GetValue(int keyIndex, int localeIndex) {
            ref var valueRow = ref _valueRows[keyIndex];
            return valueRow.values[localeIndex];
        }

        public void SetValue(string key, string value, Locale locale) {
            int localeIndex = GetOrAddLocale(locale);
            int keyIndex = GetOrAddKey(key);
            
            ref var valueRow = ref _valueRows[keyIndex];
            valueRow.values[localeIndex] = value;
        }

        public void ClearAll() {
            _locales = Array.Empty<Locale>();
            _valueRows = Array.Empty<ValueRow>();

#if UNITY_EDITOR
            _keyHashToIndexMap = null;      
#endif
        }

        private int GetOrAddLocale(Locale locale) {
            int count = _locales?.Length ?? 0;
            
            for (int i = 0; i < count; i++) {
                if (_locales![i] == locale) return i; 
            }
            
#if UNITY_EDITOR
            _allowInvalidateMap = false;
#endif
            
            Array.Resize(ref _locales, count + 1);
            _locales[count] = locale;

            for (int i = 0; i < _valueRows?.Length; i++) {
                ref var valueRow = ref _valueRows[i];
                Array.Resize(ref valueRow.values, count + 1);
            }

#if UNITY_EDITOR
            _allowInvalidateMap = true;
#endif
            
            return count;
        }
        
        private int GetOrAddKey(string key) {
#if UNITY_EDITOR
            if (_keyHashToIndexMap?.TryGetValue(Animator.StringToHash(key), out int index) ?? false) {
                return index;
            }
            
            _keyHashToIndexMap ??= new Dictionary<int, int>();
#endif
            
            int count = _valueRows?.Length ?? 0;
            
            for (int i = 0; i < count; i++) {
                ref var valueRow = ref _valueRows![i];
                if (valueRow.key != key) continue;
                    
#if UNITY_EDITOR
                _keyHashToIndexMap.Add(Animator.StringToHash(key), i);
#endif

                return i;
            }

#if UNITY_EDITOR
            _allowInvalidateMap = false;
#endif
            
            Array.Resize(ref _valueRows, count + 1);
            _valueRows[count] = new ValueRow { key = key, values = new string[_locales?.Length ?? 0] };

#if UNITY_EDITOR
            _allowInvalidateMap = true;
            _keyHashToIndexMap.Add(Animator.StringToHash(key), count);
#endif
            
            return count;
        }

#if UNITY_EDITOR
        private Dictionary<int, int> _keyHashToIndexMap;
        private bool _allowInvalidateMap = true;

        private void OnValidate() {
            if (_allowInvalidateMap) _keyHashToIndexMap = null;
        }
#endif
    }
    
}