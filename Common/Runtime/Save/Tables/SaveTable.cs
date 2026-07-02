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

        public void SetData<S>(long id, S data) {
            if (this is not SaveTable<S> table) return;

            table._dataMap[id] = data;
        }

        public void RemoveData(long id) {
            _dataMap.Remove(id);
        }

        public void Clear() {
            _dataMap.Clear();
        }
    }
    
}