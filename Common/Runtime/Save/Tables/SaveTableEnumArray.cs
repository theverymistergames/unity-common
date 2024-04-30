using System;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Common.Save.Tables {

    [Serializable]
    [SaveTable(typeof(Enum[]))]
    public sealed class SaveTableEnumArray : ISaveTable {

        [SerializeField] private Map<long, SaveRecord<ulong[]>> _dataMap = new();

        public Type GetElementType() => typeof(Enum[]);

        public void PrepareRecord(string id, int index) {
            long key = NumberExtensions.TwoIntsAsLong(Animator.StringToHash(id), index);
            if (!_dataMap.ContainsKey(key)) _dataMap[key] = new SaveRecord<ulong[]> { id = id, index = index };
        }
        
        public bool TryGetData<S>(long id, out S data) {
            if (_dataMap.TryGetValue(id, out var record)) {
                var elementType = typeof(S).GetElementType() ?? typeof(ulong);    
                var array = Array.CreateInstance(elementType, record.data.Length);

                for (int i = 0; i < record.data.Length; i++) {
                    if (Enum.ToObject(elementType, record.data[i]) is {} e) array.SetValue(e, i);
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
            record.data = new ulong[array.Length];
            
            for (int i = 0; i < array.Length; i++) {
                if (Convert.ChangeType(array.GetValue(i), typeof(ulong)) is {} e) record.data[i] = (ulong) e;
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
