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

        public bool SetData<S>(long id, S data) {
            if (data is not Array array) return false;
            
            var dataArray = new Object[array.Length];
            _dataMap[id] = dataArray;
            
            for (int i = 0; i < array.Length; i++) {
                if (array.GetValue(i) is Object v) dataArray[i] = v;
            }
            
            return true;
        }

        public bool TryGetDataBoxed(long id, out object data) {
            if (_dataMap.TryGetValue(id, out var value)) {
                data = value;
                return true;
            }

            data = null;
            return false;
        }
        
        public bool SetDataBoxed(long id, object data) {
            if (data is not Array array) return false;
            
            var dataArray = new Object[array.Length];
            _dataMap[id] = dataArray;
            
            for (int i = 0; i < array.Length; i++) {
                if (array.GetValue(i) is Object v) dataArray[i] = v;
            }
            
            return true;
        }

        public bool RemoveData(long id) {
            return _dataMap.Remove(id);
        }

        public bool ContainsData(long id) {
            return _dataMap.ContainsKey(id);
        }

        public bool IsEmpty() {
            return _dataMap.Count == 0;
        }

        public void Clear() {
            _dataMap.Clear();
        }
        
        public string GetSerializedPropertyPath(long hash) {
            int index = _dataMap.FirstIndexOf(hash, (h, e) => e.key == h);
            return index >= 0
                ? $"{nameof(_dataMap)}._entries.Array.data[{index}].value"
                : null;
        }
    }

}
