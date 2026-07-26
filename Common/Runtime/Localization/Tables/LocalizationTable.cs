using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using UnityEngine;
using UnityEngine.Pool;

namespace MisterGames.Common.Localization {
    
    public sealed class LocalizationTable : ILocalizationTable, IDisposable, IDisposableHandler {

        private readonly ILocalizationTableStorage _storage;
        private readonly Dictionary<int, int> _keyHashToIndexMap;
        private readonly Dictionary<int, int> _localeHashToIndexMap;
        private readonly HashSet<int> _disposablesSet = new();
        private int _lastDisposableId;

        public LocalizationTable(ILocalizationTableStorage storage) {
            _storage = storage;
            _keyHashToIndexMap = CreateKeyIndexMap(storage);
            _localeHashToIndexMap = CreateLocaleIndexMap(storage);
        }

        public void Dispose() {
            DictionaryPool<int, int>.Release(_keyHashToIndexMap);
            DictionaryPool<int, int>.Release(_localeHashToIndexMap);
        }

        public bool CanUnload() {
            return _disposablesSet.Count <= 0;
        }

        public bool TryGetKey(int keyHash, out string value) {
            if (_keyHashToIndexMap.TryGetValue(keyHash, out int keyIndex)) {
                value = _storage.GetKey(keyIndex);
                return true;
            }
            
            value = null;
            return false;
        }

        public bool TryGetValue<T>(int keyHash, int localeHash, out T value) {
            if (_localeHashToIndexMap.TryGetValue(localeHash, out int localeIndex) &&
                _keyHashToIndexMap.TryGetValue(keyHash, out int keyIndex) && 
                _storage is ILocalizationTableStorage<T> storageT) 
            {
                return storageT.TryGetValue(keyIndex, localeIndex, out value);
            }

            value = default;
            return false;
        }

        public bool TryGetDisposableValue<T>(int keyHash, int localeHash, out Disposable<T> disposableValue) {
            if (TryGetValue(keyHash, localeHash, out T value)) {
                disposableValue = CreateDisposableValue(value);
                return true;
            }

            disposableValue = default;
            return false;
        }

        private Disposable<T> CreateDisposableValue<T>(T value) {
            int id = _lastDisposableId.IncrementUncheckedRef();
            _disposablesSet.Add(id);
            
            return new Disposable<T>(value, id, this);
        }
        
        void IDisposableHandler.NotifyDispose(int id) {
            _disposablesSet.Remove(id);
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