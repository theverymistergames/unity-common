using System;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Common.Save.Tables {
    
    [Serializable]
    public abstract class SaveTableByRef<T> : ISaveTable {
        
        [SerializeField] private Map<long, SaveRecordByRef<T>> _dataMap = new();

        public Type GetElementType() {
            return typeof(T);
        }

        public void PrepareRecord(ISaveSystem saveSystem, long id) {
            if (_dataMap.ContainsKey(id)) return;
            
            NumberExtensions.LongAsTwoInts(id, out int hash, out int index);
            
            _dataMap[id] = new SaveRecordByRef<T> { id = saveSystem.GetPropertyName(hash), index = index };
        }

        public void FetchRecords(ISaveSystem saveSystem) {
            foreach (var record in _dataMap.Values) {
                saveSystem.CreateProperty(record.id);
            }
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
            if (this is not SaveTableByRef<S> table || !table._dataMap.ContainsKey(id)) return;
            
            ref var record = ref table._dataMap.Get(id);
            record.data = data;
        }

        public void RemoveData(long id) {
            _dataMap.Remove(id);
        }

        public void Clear() {
            _dataMap.Clear();
        }
    }
    
}