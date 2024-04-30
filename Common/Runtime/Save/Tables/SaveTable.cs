using System;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Common.Save.Tables {
    
    [Serializable]
    public abstract class SaveTable<T> : ISaveTable {
        
        [SerializeField] private Map<long, SaveRecord<T>> _dataMap = new();

        public Type GetElementType() {
            return typeof(T);
        }

        public void PrepareRecord(string id, int index) {
            long key = NumberExtensions.TwoIntsAsLong(Animator.StringToHash(id), index);
            if (!_dataMap.ContainsKey(key)) _dataMap[key] = new SaveRecord<T> { id = id, index = index };
        }

        public bool TryGetData<S>(long id, out S data) {
            if (this is not SaveTable<S> table || !table._dataMap.TryGetValue(id, out var record)) {
                data = default;
                return false;
            }
            
            data = record.data;
            return true;
        }

        public void SetData<S>(long id, S data) {
            if (this is not SaveTable<S> table || !table._dataMap.ContainsKey(id)) return;
            
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