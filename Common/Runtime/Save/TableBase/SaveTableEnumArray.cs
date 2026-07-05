using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Common.Save.Tables {

    [Serializable]
    public abstract class SaveTableEnumArray<TKey> : ISaveTable<TKey> where TKey : IEquatable<TKey> {

        [SerializeField] private SerializedDictionary<TKey, ulong[]> _dataMap = new();

        public Type GetKeyType() {
            return typeof(TKey);
        }

        public Type GetValueType() {
            return typeof(Enum[]);
        }

        public bool TryGetData<V>(TKey key, out V data) {
            if (_dataMap.TryGetValue(key, out ulong[] record)) {
                var elementType = typeof(V).GetElementType() ?? typeof(ulong);    
                var array = Array.CreateInstance(elementType, record?.Length ?? 0);

                for (int i = 0; i < record?.Length; i++) {
                    if (Enum.ToObject(elementType, record[i]) is {} e) array.SetValue(e, i);
                }
                
                data = array is V s ? s : default;
                return true;
            }
            
            data = default;
            return false;
        }

        public bool SetData<S>(TKey key, S data) {
            if (data is not Array array) return false;
            
            ulong[] dataArray = new ulong[array.Length];
            _dataMap[key] = dataArray;
            
            for (int i = 0; i < array.Length; i++) {
                if (Convert.ChangeType(array.GetValue(i), typeof(ulong)) is {} e) dataArray[i] = (ulong) e;
            }
            
            return true;
        }

        public bool TryGetDataBoxed(TKey key, out object data) {
            if (_dataMap.TryGetValue(key, out ulong[] value)) {
                data = value;
                return true;
            }

            data = null;
            return false;
        }
        
        public bool SetDataBoxed(TKey key, object data) {
            if (data is not Array array) return false;
            
            ulong[] dataArray = new ulong[array.Length];
            _dataMap[key] = dataArray;
            
            for (int i = 0; i < array.Length; i++) {
                if (Convert.ChangeType(array.GetValue(i), typeof(ulong)) is {} e) dataArray[i] = (ulong) e;
            }
            
            return true;
        }

        public bool RemoveData(TKey key) {
            return _dataMap.Remove(key);
        }

        public bool ContainsData(TKey key) {
            return _dataMap.ContainsKey(key);
        }

        public bool IsEmpty() {
            return _dataMap.Count == 0;
        }

        public void Clear() {
            _dataMap.Clear();
        }
        
        public string GetSerializedPropertyPath(TKey key) {
            int index = _dataMap.FirstIndexOf(key, (h, e) => e.key.Equals(h));
            return index >= 0
                ? $"{nameof(_dataMap)}._entries.Array.data[{index}].value"
                : null;
        }
    }
    
}
