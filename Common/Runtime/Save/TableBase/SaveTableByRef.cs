using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Common.Save.Tables {
    
    [Serializable]
    public abstract class SaveTableByRef<TKey, TValue> : ISaveTable<TKey> where TKey : IEquatable<TKey> {
        
        [SerializeField] private SerializedDictionaryByRef<TKey, TValue> _dataMap = new();

        public Type GetKeyType() {
            return typeof(TKey);
        }

        public Type GetValueType() {
            return typeof(TValue);
        }

        public bool TryGetData<V>(TKey key, out V data) {
            if (this is SaveTableByRef<TKey, V> table && table._dataMap.TryGetValue(key, out data)) return true;
            
            data = default;
            return false;
        }

        public bool SetData<V>(TKey key, V data) {
            if (this is not SaveTableByRef<TKey, V> table) return false;
            
            table._dataMap[key] = data;
            return true;
        }

        public bool TryGetDataBoxed(TKey key, out object data) {
            if (_dataMap.TryGetValue(key, out var value)) {
                data = value;
                return true;
            }

            data = null;
            return false;
        }
        
        public bool SetDataBoxed(TKey key, object data) {
            if (data is not TValue t) return false;
            
            _dataMap[key] = t;
            return true;
        }
        
        public bool RemoveData(TKey key) {
            return _dataMap.Remove(key);
        }

        public bool ContainsData(TKey key) {
            return _dataMap.ContainsKey(key);
        }

        public bool IsEmpty() {
            return _dataMap.Count == 0;
        }

        public void Clear() {
            _dataMap.Clear();
        }
        
        public string GetSerializedPropertyPath(TKey key) {
            int index = _dataMap.FirstIndexOf(key, (h, e) => e.key.Equals(h));
            return index >= 0
                ? $"{nameof(_dataMap)}._entries.Array.data[{index}].value"
                : null;
        }
    }
    
}