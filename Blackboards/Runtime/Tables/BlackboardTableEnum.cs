﻿using System;
using MisterGames.Blackboards.Core;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blackboards.Tables {

    [Serializable]
    [BlackboardTable(typeof(Enum))]
    public sealed class BlackboardTableEnum : IBlackboardTable {

        [SerializeField] private Map<int, BlackboardValue<ulong>> _map = new Map<int, BlackboardValue<ulong>>();

        public int Count => _map.Count;

        public T Get<T>(int hash) {
            return _map.TryGetValue(hash, out var v) && Enum.ToObject(typeof(T), v.value) is T t
                ? t
                : default;
        }

        public void Set<T>(int hash, T value) {
            if (!_map.ContainsKey(hash)) return;

            _map[hash] = Convert.ChangeType(value, typeof(ulong)) is {} e
                ? new BlackboardValue<ulong>((ulong) e)
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
            _map[hash] = value is BlackboardValue<ulong> v ? v : default;
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
