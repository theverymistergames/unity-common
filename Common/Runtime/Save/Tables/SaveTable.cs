using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Common.Save.Tables {
    
    [Serializable]
    public abstract class SaveTable<T> : ISaveTable {
        
        [SerializeField] private SerializedDictionary<long, T> _dataMap = new();

        public Type GetElementType() {
            return typeof(T);
        }

        public bool TryGetData<S>(long id, out S data) {
            if (this is SaveTable<S> table && table._dataMap.TryGetValue(id, out data)) return true;
            
            data = default;
            return false;
        }

        public bool SetData<S>(long id, S data) {
            if (this is not SaveTable<S> table) return false;

            table._dataMap[id] = data;
            return true;
        }

        public bool TryGetDataBoxed(long id, out object data) {
            if (_dataMap.TryGetValue(id, out var value)) {
                data = value;
                return true;
            }

            data = null;
            return false;
        }
        
        public bool SetDataBoxed(long id, object data) {
            if (data is not T t) return false;
            
            _dataMap[id] = t;
            return true;
        }

        public bool RemoveData(long id) {
            return _dataMap.Remove(id);
        }

        public bool ContainsData(long id) {
            return _dataMap.ContainsKey(id);
        }

        public bool IsEmpty() {
            return _dataMap.Count == 0;
        }

        public void Clear() {
            _dataMap.Clear();
        }
        
        public string GetSerializedPropertyPath(long hash) {
            int index = _dataMap.FirstIndexOf(hash, (h, e) => e.key == h);
            return index >= 0
                ? $"{nameof(_dataMap)}._entries.Array.data[{index}].value"
                : null;
        }
    }
    
}