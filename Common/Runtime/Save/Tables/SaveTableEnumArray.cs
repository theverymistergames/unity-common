using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Common.Save.Tables {

    [Serializable]
    [SaveTable(typeof(Enum[]))]
    public sealed class SaveTableEnumArray : ISaveTable {

        [SerializeField] private Map<long, ulong[]> _dataMap = new();

        public Type GetElementType() => typeof(Enum[]);
        
        public bool TryGetData<S>(long id, out S data) {
            if (_dataMap.TryGetValue(id, out ulong[] record)) {
                var elementType = typeof(S).GetElementType() ?? typeof(ulong);    
                var array = Array.CreateInstance(elementType, record?.Length ?? 0);

                for (int i = 0; i < record?.Length; i++) {
                    if (Enum.ToObject(elementType, record[i]) is {} e) array.SetValue(e, i);
                }
                
                data = array is S s ? s : default;
                return true;
            }
            
            data = default;
            return false;
        }

        public void SetData<S>(long id, S data) {
            if (data is not Array array) return;
            
            ulong[] dataArray = new ulong[array.Length];
            _dataMap[id] = dataArray;
            
            for (int i = 0; i < array.Length; i++) {
                if (Convert.ChangeType(array.GetValue(i), typeof(ulong)) is {} e) dataArray[i] = (ulong) e;
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
