using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Common.Strings;
using MisterGames.Common.Types;
using UnityEngine;

namespace MisterGames.Common.Save.Storages {
    
    [Serializable]
    public sealed class SaveBoard {

        [SerializeField] private SaveStorage _saveStorage = new();
        [SerializeField] private SerializedDictionary<long, SaveProperty> _propertyMap = new();
        [SerializeField] private List<long> _propertyList = new();
        
        public IReadOnlyList<long> PropertyList => _propertyList;
        
        public static int StringToHash(string name) {
            return name == null ? 0 : Animator.StringToHash(name);
        }
        
        public bool TryGet<T>(long hash, out T data) {
            data = default;
            return _propertyMap.TryGetValue(hash, out var property) &&
                   _saveStorage.GetTable(property.type.ToType()) is { } table &&
                   table.TryGetData(hash, out data);
        }
        
        public T Get<T>(long hash) {
            return TryGet(hash, out T data) ? data : default;
        }

        public T GetOrDefault<T>(long hash, T defaultValue = default) {
            return TryGet(hash, out T data) ? data : defaultValue;
        }
        
        public bool Set<T>(long hash, T value) {
            return _propertyMap.TryGetValue(hash, out var property) &&
                   _saveStorage.GetTable(property.type.ToType()) is { } table &&
                   table.SetData(hash, value);
        }

        public bool Contains(long hash) {
            return _propertyMap.TryGetValue(hash, out var property) && 
                   _saveStorage.GetTable(property.type.ToType()) is { } table && 
                   table.ContainsData(hash);
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

        public bool TryGetProperty(long hash, out SaveProperty property) {
            return _propertyMap.TryGetValue(hash, out property);
        }

        public bool TryGetPropertyValueBoxed(long hash, out object value) {
            value = null;
            return _propertyMap.TryGetValue(hash, out var property) &&
                   _saveStorage.GetTable(property.type.ToType()) is { } table &&
                   table.TryGetDataBoxed(hash, out value);
        }

        public bool SetPropertyValueBoxed(long hash, object value) {
            return _propertyMap.TryGetValue(hash, out var property) &&
                   _saveStorage.GetTable(property.type.ToType()) is { } table &&
                   table.SetDataBoxed(hash, value);
        }
        
        public bool RemovePropertyValue(long hash) {
            if (!_propertyMap.TryGetValue(hash, out var property) ||
                _saveStorage.GetTable(property.type.ToType()) is not { } table) 
            {
                return false;
            }
            
            bool removed = table.RemoveData(hash);
            if (table.IsEmpty()) _saveStorage.RemoveTable(property.type.ToType());
            return removed;
        }

        public bool TrySetPropertyName(long hash, string newName) {
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

        public string GetSerializedPropertyPath(long hash) {
            if (!_propertyMap.TryGetValue(hash, out var property) || 
                _saveStorage.GetTable(property.type.ToType()) is not { } table || 
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