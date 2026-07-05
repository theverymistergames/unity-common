using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Common.Save.Tables;
using MisterGames.Common.Strings;
using MisterGames.Common.Types;
using UnityEngine;

namespace MisterGames.Common.Save.Storages {
    
    [Serializable]
    public sealed class SaveBoard {

        [SerializeField] private SaveStorage<int> _saveStorage = new();
        [SerializeField] private SerializedDictionary<int, SaveProperty> _propertyMap = new();
        [SerializeField] private List<int> _propertyList = new();
        
        public IReadOnlyList<int> PropertyList => _propertyList;
        
        public static int StringToHash(string name) {
            return name == null ? 0 : Animator.StringToHash(name);
        }
        
        public bool TryGet<T>(int key, out T data) {
            data = default;
            return _saveStorage.GetTable<T>() is { } table && table.TryGetData(key, out data);
        }
        
        public T Get<T>(int key) {
            return TryGet(key, out T data) ? data : default;
        }

        public T GetOrDefault<T>(int key, T defaultValue = default) {
            return TryGet(key, out T data) ? data : defaultValue;
        }
        
        public bool Set<T>(int key, T value) {
            return _saveStorage.GetTable<T>() is { } table && table.SetData(key, value);
        }

        public bool Contains<T>(int key) {
            return _saveStorage.GetTable<T>() is { } table && table.ContainsData(key);
        }

        public void Clear() {
            _saveStorage.Clear();
            _propertyMap.Clear();
        }
        
        public bool TryAddProperty(string name, Type type) {
            name = ValidateName(name);
            int hash = StringToHash(name);
            if (_propertyMap.ContainsKey(hash)) return false;
            
            var table = _saveStorage.GetOrCreateTable(type);
            if (table == null) return false;

            var property = new SaveProperty {
                name = name,
                type = new SerializedType(type),
            };

            _propertyMap.Add(hash, property);
            return true;
        }

        public bool TryGetProperty(int hash, out SaveProperty property) {
            return _propertyMap.TryGetValue(hash, out property);
        }

        public bool TryGetPropertyValueBoxed(int hash, out object value) {
            value = null;
            return _propertyMap.TryGetValue(hash, out var property) &&
                   _saveStorage.GetTable(property.type.ToType()) is ISaveTable<int> table &&
                   table.TryGetDataBoxed(hash, out value);
        }

        public bool SetPropertyValueBoxed(int hash, object value) {
            return _propertyMap.TryGetValue(hash, out var property) &&
                   _saveStorage.GetTable(property.type.ToType()) is ISaveTable<int> table &&
                   table.SetDataBoxed(hash, value);
        }
        
        public bool RemovePropertyValue(int hash) {
            if (!_propertyMap.TryGetValue(hash, out var property) ||
                _saveStorage.GetTable(property.type.ToType()) is not ISaveTable<int> table) 
            {
                return false;
            }
            
            bool removed = table.RemoveData(hash);
            if (table.IsEmpty()) _saveStorage.RemoveTable(property.type.ToType());
            return removed;
        }

        public bool TrySetPropertyName(int hash, string newName) {
            if (!_propertyMap.TryGetValue(hash, out var property)) return false;

            newName = ValidateName(newName);
            int newHash = StringToHash(newName);
            if (_propertyMap.ContainsKey(newHash)) return false;

            property.name = newName;

            _propertyMap.Remove(hash);
            _propertyMap[newHash] = property;

            for (int i = 0; i < _propertyList.Count; i++) {
                if (_propertyList[i] != hash) continue;

                _propertyList[i] = newHash;
                break;
            }

            TryGetPropertyValueBoxed(hash, out object value);
            RemovePropertyValue(hash);
            SetPropertyValueBoxed(newHash, value);

            return true;
        }

        public bool TrySetPropertyIndex(int hash, int newIndex) {
            if (newIndex < 0) return false;

            int oldIndex = -1;
            for (int i = 0; i < _propertyList.Count; i++) {
                if (_propertyList[i] != hash) continue;

                oldIndex = i;
                break;
            }

            if (oldIndex < 0 || oldIndex == newIndex) return false;

            if (newIndex >= _propertyList.Count) {
                _propertyList.RemoveAt(oldIndex);
                _propertyList.Add(hash);
                return true;
            }
            
            _propertyList.Insert(newIndex, hash);
            
            if (oldIndex > newIndex) oldIndex++;
            _propertyList.RemoveAt(oldIndex);
            
            return true;
        }

        public string GetSerializedPropertyPath(int hash) {
            if (!_propertyMap.TryGetValue(hash, out var property) || 
                _saveStorage.GetTable(property.type.ToType()) is not ISaveTable<int> table || 
                table.GetSerializedPropertyPath(hash) is not { } valuePath || 
                _saveStorage.GetSerializedPropertyPath(property.type.ToType()) is not { } tablePath) 
            {
                return null;
            }

            return $"{nameof(_saveStorage)}.{tablePath}.{valuePath}";
        }

        private string ValidateName(string name) {
            int hash = StringToHash(name);
            if (!_propertyMap.ContainsKey(hash)) return name;

            int count = 1;
            string pattern = $@"{name} \([0-9]+\)";

            for (int i = 0; i < _propertyList.Count; i++) {
                if (_propertyMap[_propertyList[i]].name.HasRegexPattern(pattern)) count++;
            }

            return $"{name} ({count})";
        }
    }
    
}