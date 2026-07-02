using System;
using MisterGames.Common.Data;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Common.Save.Tables {

    [Serializable]
    [SaveTable(typeof(Object[]))]
    public sealed class SaveTableUnityObjectArray : ISaveTable {

        [SerializeField] private SerializedDictionary<long, Object[]> _dataMap = new();

        public Type GetElementType() => typeof(Object[]);

        public bool TryGetData<S>(long id, out S data) {
            if (_dataMap.TryGetValue(id, out var record)) {
                var elementType = typeof(S).GetElementType() ?? typeof(Object);    
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
            
            var dataArray = new Object[array.Length];
            _dataMap[id] = dataArray;
            
            for (int i = 0; i < array.Length; i++) {
                if (array.GetValue(i) is Object v) dataArray[i] = v;
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
