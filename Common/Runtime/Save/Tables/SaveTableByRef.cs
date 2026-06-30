using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Common.Save.Tables {
    
    [Serializable]
    public abstract class SaveTableByRef<T> : ISaveTable {
        
        [SerializeField] private Map<long, SaveRecordByRef<T>> _dataMap = new();

        public Type GetElementType() {
            return typeof(T);
        }

        public bool TryGetData<S>(long id, out S data) {
            if (this is not SaveTableByRef<S> table || !table._dataMap.TryGetValue(id, out var record)) {
                data = default;
                return false;
            }
            
            data = record.data;
            return true;
        }

        public void SetData<S>(long id, S data) {
            if (this is not SaveTableByRef<S> table) return;
            
            table._dataMap[id] = new SaveRecordByRef<S>(data);
        }

        public void RemoveData(long id) {
            _dataMap.Remove(id);
        }

        public void Clear() {
            _dataMap.Clear();
        }
    }
    
}