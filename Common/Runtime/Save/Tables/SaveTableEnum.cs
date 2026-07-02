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

        public void SetData<S>(long id, S data) {
            if (Convert.ChangeType(data, typeof(ulong)) is not {} e) return;
            
            _dataMap[id] = (ulong) e;
        }

        public void RemoveData(long id) {
            _dataMap.Remove(id);
        }

        public void Clear() {
            _dataMap.Clear();
        }
    }

}
