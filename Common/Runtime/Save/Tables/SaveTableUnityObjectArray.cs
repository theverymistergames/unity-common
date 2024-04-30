using System;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Common.Save.Tables {

    [Serializable]
    [SaveTable(typeof(Object[]))]
    public sealed class SaveTableUnityObjectArray : ISaveTable {

        [SerializeField] private Map<long, SaveRecord<object[]>> _dataMap = new();

        public Type GetElementType() => typeof(Object[]);

        public void PrepareRecord(ISaveSystem saveSystem, long id) {
            if (_dataMap.ContainsKey(id)) return;
            
            NumberExtensions.LongAsTwoInts(id, out int hash, out int index);
            
            _dataMap[id] = new SaveRecord<object[]> { id = saveSystem.GetPropertyName(hash), index = index };
        }

        public void FetchRecords(ISaveSystem saveSystem) {
            foreach (var record in _dataMap.Values) {
                saveSystem.CreateProperty(record.id);
            }
        }

        public bool TryGetData<S>(long id, out S data) {
            if (_dataMap.TryGetValue(id, out var record)) {
                var elementType = typeof(S).GetElementType() ?? typeof(object);    
                var array = Array.CreateInstance(elementType, record.data.Length);

                for (int i = 0; i < record.data.Length; i++) {
                    array.SetValue(record.data[i], i);
                }
                
                data = array is S s ? s : default;
                return true;
            }
            
            data = default;
            return false;
        }

        public void SetData<S>(long id, S data) {
            if (!_dataMap.ContainsKey(id)) return;
            
            if (data is not Array array) return;
            
            ref var record = ref _dataMap.Get(id);
            record.data = new object[array.Length];
            
            for (int i = 0; i < array.Length; i++) {
                if (array.GetValue(i) is Object v) record.data[i] = v;
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
