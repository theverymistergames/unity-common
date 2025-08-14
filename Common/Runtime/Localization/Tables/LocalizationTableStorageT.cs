using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Common.Localization {

    public abstract class LocalizationTableStorageT<T> : LocalizationTableStorageBase, ILocalizationTableStorage<T> {
        
        [LocaleFilter(LocaleFilter.All)]
        [SerializeField] private Locale[] _locales;
        [SerializeField] private ValueRow[] _valueRows;
        
        [Serializable]
        private struct ValueRow {
            public string key;
            [UseLocaleLabel] public T[] values;
        }
        
        public override int GetLocaleCount() {
            return _locales?.Length ?? 0;
        }

        public override Locale GetLocale(int localeIndex) {
            return _locales[localeIndex];
        }
        
        public override int GetKeyCount() {
            return _valueRows?.Length ?? 0;
        }

        public override bool TryGetKey(int keyHash, out string key) {
#if UNITY_EDITOR
            if (_keyHashToIndexMap?.TryGetValue(keyHash, out int index) ?? false) {
                key = _valueRows[index].key;
                return true;
            }
            
            _keyHashToIndexMap ??= new Dictionary<int, int>();
#endif
            
            int count = _valueRows?.Length ?? 0;
            
            for (int i = 0; i < count; i++) {
                ref var valueRow = ref _valueRows![i];
                int hash = Animator.StringToHash(valueRow.key);
                
#if UNITY_EDITOR
                _keyHashToIndexMap[hash] = i;
#endif
                
                if (hash != keyHash) continue;

                key = valueRow.key;
                return true;
            }

            key = null;
            return false;
        }

        public override string GetKey(int keyIndex) {
            ref var valueRow = ref _valueRows[keyIndex];
            return valueRow.key;
        }

        public bool TryGetValue(int keyIndex, int localeIndex, out T value) {
            ref var valueRow = ref _valueRows[keyIndex];
            value = valueRow.values[localeIndex];
            
            return true;
        }

        public void SetValue(string key, T value, Locale locale) {
            int localeIndex = GetOrAddLocale(locale);
            int keyIndex = GetOrAddKey(key);
            
            ref var valueRow = ref _valueRows[keyIndex];
            valueRow.values[localeIndex] = value;
        }

        public override void ClearAll() {
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
            _valueRows[count] = new ValueRow { key = key, values = new T[_locales?.Length ?? 0] };

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