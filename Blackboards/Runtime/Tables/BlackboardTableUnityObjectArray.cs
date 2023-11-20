using System;
using MisterGames.Blackboards.Core;
using MisterGames.Common.Data;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Blackboards.Tables {

    [Serializable]
    [BlackboardTable(typeof(Object[]))]
    public sealed class BlackboardTableUnityObjectArray : IBlackboardTable {

        [SerializeField] private Map<int, BlackboardValue<Object>[]> _map = new Map<int, BlackboardValue<Object>[]>();

        public int Count => _map.Count;

        public T Get<T>(int hash) {
            if (!_map.TryGetValue(hash, out var a)) return default;

            var type = typeof(T);
            if (!type.IsArray) return default;

            var elementType = typeof(T).GetElementType()!;
            var array = Array.CreateInstance(elementType, a.Length);

            for (int i = 0; i < a.Length; i++) {
                if (a[i].value is {} v && v.GetType() == elementType) array.SetValue(v, i);
            }

            return array is T t ? t : default;
        }

        public void Set<T>(int hash, T value) {
            if (!_map.ContainsKey(hash) || value is not Array a) return;

            var array = new BlackboardValue<Object>[a.Length];
            for (int i = 0; i < array.Length; i++) {
                if (a.GetValue(i) is Object v) array[i] = new BlackboardValue<Object>(v);
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

        public void SetOrAddValue(int hash, object value) {
            _map[hash] = value as BlackboardValue<Object>[];
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
