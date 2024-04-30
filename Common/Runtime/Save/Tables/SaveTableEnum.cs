using System;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Common.Save.Tables {

    [Serializable]
    [SaveTable(typeof(Enum))]
    public sealed class SaveTableEnum : ISaveTable {

        [SerializeField] private Map<long, SaveRecord<ulong>> _dataMap = new();

        public Type GetElementType() => typeof(Enum);

        public void PrepareRecord(string id, int index) {
            long key = NumberExtensions.TwoIntsAsLong(Animator.StringToHash(id), index);
            if (!_dataMap.ContainsKey(key)) _dataMap[key] = new SaveRecord<ulong> { id = id, index = index };
        }

        public bool TryGetData<S>(long id, out S data) {
            if (_dataMap.TryGetValue(id, out var record) && Enum.ToObject(typeof(S), record.data) is S s) {
                data = s;
                return true;
            }
            
            data = default;
            return false;
        }

        public void SetData<S>(long id, S data) {
            if (!_dataMap.ContainsKey(id) || Convert.ChangeType(data, typeof(ulong)) is not {} e) return;
            
            ref var record = ref _dataMap.Get(id);
            record.data = (ulong) e;
        }

        public void RemoveData(long id) {
            _dataMap.Remove(id);
        }

        public void Clear() {
            _dataMap.Clear();
        }
    }

}
