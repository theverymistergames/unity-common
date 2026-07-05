using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Common.Save.Tables {

    [Serializable]
    public abstract class SaveTableEnum<TKey> : ISaveTable<TKey> where TKey : IEquatable<TKey> {

        [SerializeField] private SerializedDictionary<TKey, ulong> _dataMap = new();

        public Type GetKeyType() {
            return typeof(TKey);
        }

        public Type GetValueType() {
            return typeof(Enum);
        }

        public bool TryGetData<V>(TKey key, out V data) {
            if (_dataMap.TryGetValue(key, out ulong record) && Enum.ToObject(typeof(V), record) is V s) {
                data = s;
                return true;
            }
            
            data = default;
            return false;
        }

        public bool SetData<V>(TKey key, V data) {
            if (Convert.ChangeType(data, typeof(ulong)) is not {} e) return false;
            
            _dataMap[key] = (ulong) e;
            return true;
        }

        public bool TryGetDataBoxed(TKey key, out object data) {
            if (_dataMap.TryGetValue(key, out ulong value)) {
                data = value;
                return true;
            }

            data = null;
            return false;
        }
        
        public bool SetDataBoxed(TKey key, object data) {
            if (Convert.ChangeType(data, typeof(ulong)) is not {} e) return false;
            
            _dataMap[key] = (ulong) e;
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
