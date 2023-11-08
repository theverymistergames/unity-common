using System;
using MisterGames.Blackboards.Core;
using MisterGames.Common.Data;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Blackboards.Tables {

    [Serializable]
    [BlackboardTable(typeof(Object))]
    public sealed class BlackboardTableUnityObject : IBlackboardTable {

        [SerializeField] private ArrayMap<int, BlackboardValue<Object>> _map;

        public int Count => _map.Count;

        public T Get<T>(int hash) {
            return _map.TryGetValue(hash, out var v) && v.value is T t ? t : default;
        }

        public void Set<T>(int hash, T value) {
            if (!_map.ContainsKey(hash)) return;

            _map[hash] = value is Object o
                ? new BlackboardValue<Object>(o)
                : default;
        }

        public bool Contains(int hash) {
            return _map.ContainsKey(hash);
        }

        public bool TryGetValue(int hash, out object value) {
            if (!_map.TryGetValue(hash, out var v)) {
                value = default;
                return false;
            }

            value = v;
            return true;
        }

        public bool TrySetValue(int hash, object value) {
            if (!_map.ContainsKey(hash)) return false;

            _map[hash] = value is BlackboardValue<Object> v ? v : default;
            return true;
        }

        public bool RemoveValue(int hash) {
            if (!_map.ContainsKey(hash)) return false;

            _map.Remove(hash);
            return true;
        }

        public string GetSerializedPropertyPath(int hash) {
            return _map.ContainsKey(hash)
                ? $"{nameof(_map)}._nodes.Array.data[{_map.IndexOf(hash)}].value"
                : null;
        }
    }

}
