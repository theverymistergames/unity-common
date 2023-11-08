using System;
using MisterGames.Blackboards.Core;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blackboards.Tables {

    [Serializable]
    [BlackboardTable(typeof(Enum[]))]
    public sealed class BlackboardTableEnumArray : IBlackboardTable {

        [SerializeField] private ArrayMap<int, BlackboardValue<ulong>[]> _map;

        public int Count => _map.Count;

        public T Get<T>(int hash) {
            if (!_map.TryGetValue(hash, out var a)) return default;

            var type = typeof(T);
            if (!type.IsArray) return default;

            var elementType = typeof(T).GetElementType()!;
            var array = Array.CreateInstance(elementType, a.Length);

            for (int i = 0; i < a.Length; i++) {
                if (Enum.ToObject(elementType, a[i].value) is {} e) array.SetValue(e, i);
            }

            return array is T t ? t : default;
        }

        public void Set<T>(int hash, T value) {
            if (!_map.ContainsKey(hash) || value is not Array a) return;

            var array = new BlackboardValue<ulong>[a.Length];
            for (int i = 0; i < array.Length; i++) {
                if (Convert.ChangeType(a.GetValue(i), typeof(ulong)) is {} e) array[i] = new BlackboardValue<ulong>((ulong) e);
            }

            _map[hash] = array;
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

            _map[hash] = value as BlackboardValue<ulong>[];
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
