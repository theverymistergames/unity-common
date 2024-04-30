using System;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Common.Save.Tables {

    [Serializable]
    [SaveTable(typeof(Object[]))]
    public sealed class SaveTableUnityObjectArray : ISaveTable {

        [SerializeField] private Map<long, SaveRecord<Object[]>> _dataMap = new();

        public Type GetElementType() => typeof(Object[]);

        public void PrepareRecord(string id, int index) {
            long key = NumberExtensions.TwoIntsAsLong(Animator.StringToHash(id), index);
            if (!_dataMap.ContainsKey(key)) _dataMap[key] = new SaveRecord<Object[]> { id = id, index = index };
        }

        public bool TryGetData<S>(long id, out S data) {
            if (_dataMap.TryGetValue(id, out var record)) {
                var elementType = typeof(S).GetElementType() ?? typeof(Object);    
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
            record.data = new Object[array.Length];
            
            for (int i = 0; i < array.Length; i++) {
                if (array.GetValue(i) is Object v) record.data[i] = v;
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
