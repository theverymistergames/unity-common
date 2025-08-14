using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace MisterGames.Common.Localization {
    
    public sealed class LocalizationTable : ILocalizationTable, IDisposable {

        private readonly ILocalizationTableStorage _storage;
        private readonly Dictionary<int, int> _keyHashToIndexMap;
        private readonly Dictionary<int, int> _localeHashToIndexMap;

        public LocalizationTable(ILocalizationTableStorage storage) {
            _storage = storage;
            _keyHashToIndexMap = CreateKeyIndexMap(storage);
            _localeHashToIndexMap = CreateLocaleIndexMap(storage);
        }

        public void Dispose() {
            DictionaryPool<int, int>.Release(_keyHashToIndexMap);
            DictionaryPool<int, int>.Release(_localeHashToIndexMap);
        }

        public bool ContainsKey(int keyHash) {
            return _keyHashToIndexMap.ContainsKey(keyHash);
        }

        public bool TryGetValue(int keyHash, int localeHash, out string value) {
            if (_localeHashToIndexMap.TryGetValue(localeHash, out int localeIndex) &&
                _keyHashToIndexMap.TryGetValue(keyHash, out int keyIndex)) 
            {
                value = _storage.GetValue(keyIndex, localeIndex);
                return !string.IsNullOrEmpty(value);
            }

            value = null;
            return false;
        }
        
        private static Dictionary<int, int> CreateKeyIndexMap(ILocalizationTableStorage storage) {
            int keyCount = storage.GetKeyCount();
            var map = DictionaryPool<int, int>.Get();

            for (int i = 0; i < keyCount; i++) {
                string key = storage.GetKey(i);
                if (string.IsNullOrWhiteSpace(key)) continue;

                map[Animator.StringToHash(key)] = i;
            }
            
            return map;
        }
        
        private static Dictionary<int, int> CreateLocaleIndexMap(ILocalizationTableStorage storage) {
            int localeCount = storage.GetLocaleCount();
            var map = DictionaryPool<int, int>.Get();

            for (int i = 0; i < localeCount; i++) {
                map[storage.GetLocale(i).Hash] = i;
            }
            
            return map;
        }
    }
    
}