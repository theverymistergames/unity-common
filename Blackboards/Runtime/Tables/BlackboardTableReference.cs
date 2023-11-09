using System;
using MisterGames.Blackboards.Core;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blackboards.Tables {

    [Serializable]
    [BlackboardTable(typeof(object))]
    public sealed class BlackboardTableReference : IBlackboardTable {

        [SerializeField] private ArrayMap<int, BlackboardReference> _map = new ArrayMap<int, BlackboardReference>();

        public int Count => _map.Count;

        public T Get<T>(int hash) {
            return _map.TryGetValue(hash, out var v) && v.value is T t ? t : default;
        }

        public void Set<T>(int hash, T value) {
            if (!_map.ContainsKey(hash)) return;

            _map[hash] = value is object o
                ? new BlackboardReference(o)
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

        public void SetOrAddValue(int hash, object value) {
            _map[hash] = value is BlackboardReference v ? v : default;
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
