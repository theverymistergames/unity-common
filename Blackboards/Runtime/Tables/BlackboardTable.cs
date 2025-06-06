using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blackboards.Tables {

    [Serializable]
    public abstract class BlackboardTable<T> : IBlackboardTable<T> {

        [SerializeField] private Map<int, T> _map = new();

        public int Count => _map.Count;

        public V Get<V>(int hash) {
            return _map.GetValueOrDefault(hash) is V v ? v : default;
        }

        public void Set<V>(int hash, V value) {
            if (_map.ContainsKey(hash)) _map[hash] = value is T t ? t : default;
        }

        public T Get(int hash) { 
            return _map.GetValueOrDefault(hash);
        }

        public void Set(int hash, T value) {
            if (_map.ContainsKey(hash)) _map[hash] = value;
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
            _map[hash] = value is T v ? v : default;
        }

        public bool RemoveValue(int hash) {
            if (!_map.ContainsKey(hash)) return false;

            _map.Remove(hash);
            return true;
        }

        public string GetSerializedPropertyPath(int hash) {
            return _map.ContainsKey(hash)
                ? $"{nameof(_map)}._entries.Array.data[{_map.IndexOf(hash)}].value"
                : null;
        }
    }

}
