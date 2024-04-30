using System;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Common.Save.Tables {

    [Serializable]
    [SaveTable(typeof(object[]))]
    public sealed class SaveTableReferenceArray : ISaveTable {

        [SerializeField] private Map<long, SaveRecord<object[]>> _dataMap = new();

        public Type GetElementType() => typeof(object[]);

        public void PrepareRecord(string id, int index) {
            long key = NumberExtensions.TwoIntsAsLong(Animator.StringToHash(id), index);
            if (!_dataMap.ContainsKey(key)) _dataMap[key] = new SaveRecord<object[]> { id = id, index = index };
        }

        public bool TryGetData<S>(long id, out S data) {
            if (_dataMap.TryGetValue(id, out var record)) {
                var elementType = typeof(S).GetElementType() ?? typeof(object);    
                var array = Array.CreateInstance(elementType, record.data.Length);

                for (int i = 0; i < record.data.Length; i++) {
                    array.SetValue(record.data[i], i);
                }
                
                data = array is S s ? s : default;
                return true;
            }
            
            data = default;
            return false;
        }

        public void SetData<S>(long id, S data) {
            if (!_dataMap.ContainsKey(id)) return;
            
            if (data is not Array array) return;
            
            ref var record = ref _dataMap.Get(id);
            record.data = new object[array.Length];
            
            for (int i = 0; i < array.Length; i++) {
                if (array.GetValue(i) is {} v) record.data[i] = v;
            }
        }

        public void RemoveData(long id) {
            _dataMap.Remove(id);
        }

        public void Clear() {
            _dataMap.Clear();
        }
    }

}
