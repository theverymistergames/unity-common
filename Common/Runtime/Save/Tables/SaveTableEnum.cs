using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Common.Save.Tables {

    [Serializable]
    [SaveTable(typeof(Enum))]
    public sealed class SaveTableEnum : ISaveTable {

        [SerializeField] private SerializedDictionary<long, ulong> _dataMap = new();

        public Type GetElementType() => typeof(Enum);

        public bool TryGetData<S>(long id, out S data) {
            if (_dataMap.TryGetValue(id, out ulong record) && Enum.ToObject(typeof(S), record) is S s) {
                data = s;
                return true;
            }
            
            data = default;
            return false;
        }

        public bool SetData<S>(long id, S data) {
            if (Convert.ChangeType(data, typeof(ulong)) is not {} e) return false;
            
            _dataMap[id] = (ulong) e;
            return true;
        }

        public bool TryGetDataBoxed(long id, out object data) {
            if (_dataMap.TryGetValue(id, out ulong value)) {
                data = value;
                return true;
            }

            data = null;
            return false;
        }
        
        public bool SetDataBoxed(long id, object data) {
            if (Convert.ChangeType(data, typeof(ulong)) is not {} e) return false;
            
            _dataMap[id] = (ulong) e;
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
