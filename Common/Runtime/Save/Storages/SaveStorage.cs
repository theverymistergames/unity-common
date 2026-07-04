using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Common.Save.Tables;
using MisterGames.Common.Types;
using UnityEngine;

namespace MisterGames.Common.Save.Storages {
    
    [Serializable]
    public sealed class SaveStorage : ISaveStorage {
        
        [SerializeField] private SerializedTypeMapByRef<ISaveTable> _tables = new();

        public IEnumerable<ISaveTable> Tables => _tables.Values;
        
        public ISaveTable GetTable<T>() {
            return GetTable(typeof(T));
        }
        
        public ISaveTable GetTable(Type elementType) {
            if (_tables.TryGetValue(elementType, out var table) && table != null) {
                return table;
            }
            
            var baseType = SaveTableCache.GetBaseElementType(elementType);
            if (_tables.TryGetValue(baseType, out table) && table != null) {
                return table;
            }
            
            return null;
        }

        public void SetTable<T>(ISaveTable value) {
            SetTable(typeof(T), value);
        }

        public void SetTable(Type elementType, ISaveTable value) {
            _tables[SaveTableCache.GetBaseElementType(elementType)] = value;
        }

        public ISaveTable GetOrCreateTable<T>() {
            return GetOrCreateTable(typeof(T));
        }
        
        public ISaveTable GetOrCreateTable(Type elementType) {
            if (_tables.TryGetValue(elementType, out var table) && table != null) {
                return table;
            }
            
            var baseType = SaveTableCache.GetBaseElementType(elementType);
            if (_tables.TryGetValue(baseType, out table) && table != null) {
                return table;
            }

            if (SaveTableCache.TryGetTableType(elementType, out var tableType) || 
                SaveTableCache.TryGetTableType(baseType, out tableType)) 
            {
                table = Activator.CreateInstance(tableType) as ISaveTable;
                _tables[baseType] = table;
            }

            return table;
        }

        public bool RemoveTable<T>() {
            return RemoveTable(typeof(T));
        }
        
        public bool RemoveTable(Type elementType) {
            return _tables.Remove(SaveTableCache.GetBaseElementType(elementType));
        }

        public void Clear() {
            foreach (var table in _tables.Values) {
                table.Clear();
            }

            _tables.Clear();
        }

        public string GetSerializedPropertyPath(Type elementType) {
            int index = _tables.FirstIndexOf(new SerializedType(elementType), (h, e) => e.key == h);
            return index >= 0
                ? $"{nameof(_tables)}._entries.Array.data[{index}].value"
                : null;
        }
    }
    
}