using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blackboards.Tables {

    [Serializable]
    public abstract class BlackboardTable<V> : IBlackboardTable {

        [SerializeField] private ArrayMap<int, V> _map = new ArrayMap<int, V>();

        public int Count => _map.Count;

        public T Get<T>(int hash) {
            return _map.TryGetValue(hash, out var v) && v is T t ? t : default;
        }

        public void Set<T>(int hash, T value) {
            if (_map.ContainsKey(hash)) _map[hash] = value is V v ? v : default;
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

        public void SetOrAddValue(int hash, object value) {
            _map[hash] = value is V v ? v : default;
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
