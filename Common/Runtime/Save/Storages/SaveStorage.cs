using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Common.Save.Tables;
using MisterGames.Common.Types;
using UnityEngine;

namespace MisterGames.Common.Save.Storages {
    
    [Serializable]
    public sealed class SaveStorage<TKey> : ISaveStorage<TKey> where TKey : IEquatable<TKey> {
        
        [SerializeField] private SerializedTypeMapByRef<ISaveTable> _tables = new();

        public IEnumerable<ISaveTable> Tables => _tables.Values;
        
        public ISaveTable<TKey> GetTable<T>() {
            return GetTable(typeof(T)) as ISaveTable<TKey>;
        }
        
        public ISaveTable GetTable(Type valueType) {
            if (_tables.TryGetValue(valueType, out var table) && table != null) {
                return table;
            }
            
            var baseType = SaveTableCache.GetBaseElementType(valueType);
            if (_tables.TryGetValue(baseType, out table) && table != null) {
                return table;
            }
            
            return null;
        }

        public void SetTable<T>(ISaveTable<TKey> value) {
            SetTable(typeof(T), value);
        }

        public void SetTable(Type valueType, ISaveTable value) {
            _tables[SaveTableCache.GetBaseElementType(valueType)] = value;
        }

        public ISaveTable<TKey> GetOrCreateTable<T>() {
            return GetOrCreateTable(typeof(T)) as ISaveTable<TKey>;
        }
        
        public ISaveTable GetOrCreateTable(Type valueType) {
            if (_tables.TryGetValue(valueType, out var table) && table != null) {
                return table;
            }
            
            var baseType = SaveTableCache.GetBaseElementType(valueType);
            if (_tables.TryGetValue(baseType, out table) && table != null) {
                return table;
            }

            var keyType = typeof(TKey);
            
            if (SaveTableCache.TryGetTableType(keyType, valueType, out var tableType) || 
                SaveTableCache.TryGetTableType(keyType, baseType, out tableType)) 
            {
                table = Activator.CreateInstance(tableType) as ISaveTable;
                _tables[baseType] = table;
            }

            return table;
        }

        public bool RemoveTable<T>() {
            return RemoveTable(typeof(T));
        }
        
        public bool RemoveTable(Type valueType) {
            return _tables.Remove(SaveTableCache.GetBaseElementType(valueType));
        }

        public void Clear() {
            foreach (var table in _tables.Values) {
                table.Clear();
            }

            _tables.Clear();
        }

        public string GetSerializedPropertyPath(Type valueType) {
            int index = _tables.FirstIndexOf(new SerializedType(valueType), (h, e) => e.key == h);
            return index >= 0
                ? $"{nameof(_tables)}._entries.Array.data[{index}].value"
                : null;
        }
    }
    
}