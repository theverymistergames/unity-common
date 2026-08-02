using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Common.Save.Tables {
    
    [Serializable]
    public abstract class SaveTableJson<TKey> : ISaveTable<TKey> where TKey : IEquatable<TKey> {
        
        [SerializeField] private SerializedDictionary<TKey, string> _dataMap = new();

        public bool TryGetData<V>(TKey key, out V data) {
            if (_dataMap.TryGetValue(key, out string json)) {
                data = JsonUtility.FromJson<V>(json);
                return true;
            }
            
            data = default;
            return false;
        }

        public bool SetData<V>(TKey key, V data) {
            _dataMap[key] = JsonUtility.ToJson(data);
            return true;
        }

        public bool TryGetDataBoxed(TKey key, out object data) {
            if (_dataMap.TryGetValue(key, out string json)) {
                data = JsonUtility.FromJson(json, typeof(object));
                return true;
            }
            
            data = null;
            return false;
        }
        
        public bool SetDataBoxed(TKey key, object data) {
            _dataMap[key] = JsonUtility.ToJson(data);
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

        public void CopyTo(ISaveTable dest) {
            if (dest is not SaveTableJson<TKey> table) return;

            foreach ((var key, string value) in _dataMap) {
                table._dataMap[key] = value;
            }
        }
    }
    
}