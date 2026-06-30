using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Common.Save.Tables {

    [Serializable]
    [SaveTable(typeof(object[]))]
    public sealed class SaveTableReferenceArray : ISaveTable {

        [SerializeField] private Map<long, object[]> _dataMap = new();

        public Type GetElementType() => typeof(object[]);

        public bool TryGetData<S>(long id, out S data) {
            if (_dataMap.TryGetValue(id, out object[] record)) {
                var elementType = typeof(S).GetElementType() ?? typeof(object);    
                var array = Array.CreateInstance(elementType, record?.Length ?? 0);

                for (int i = 0; i < record?.Length; i++) {
                    array.SetValue(record[i], i);
                }
                
                data = array is S s ? s : default;
                return true;
            }
            
            data = default;
            return false;
        }

        public void SetData<S>(long id, S data) {
            if (data is not Array array) return;
            
            object[] dataArray = new object[array.Length];
            _dataMap[id] = dataArray;
            
            for (int i = 0; i < array.Length; i++) {
                if (array.GetValue(i) is {} v) dataArray[i] = v;
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
