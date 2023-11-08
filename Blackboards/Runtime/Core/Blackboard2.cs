using System;
using System.Collections.Generic;
using MisterGames.Blackboards.Tables;
using MisterGames.Common.Data;
using MisterGames.Common.Strings;
using MisterGames.Common.Types;
using UnityEngine;

namespace MisterGames.Blackboards.Core {

    [Serializable]
    public sealed class Blackboard2 {

        [SerializeField] private List<int> _propertyList;
        [SerializeField] private SerializedDictionary<int, BlackboardProperty2> _propertyMap;
        [SerializeField] private ReferenceArrayMap<int, IBlackboardTable> _tables;
        [SerializeField] private int _tablesHead;

        public IReadOnlyList<int> Properties => _propertyList;

        public Blackboard2() {
            _propertyList = new List<int>();
            _propertyMap = new SerializedDictionary<int, BlackboardProperty2>();
            _tables = new ReferenceArrayMap<int, IBlackboardTable>();
        }

        public Blackboard2(Blackboard2 source) {
            _propertyList = new List<int>(source._propertyList);
            _propertyMap = new SerializedDictionary<int, BlackboardProperty2>(source._propertyMap);
            _tables = new ReferenceArrayMap<int, IBlackboardTable>(source._tables);
        }

        public T Get<T>(int hash) {
            return _propertyMap.TryGetValue(hash, out var property) ? _tables[property.table].Get<T>(hash) : default;
        }

        public void Set<T>(int hash, T value) {
            if (_propertyMap.TryGetValue(hash, out var property)) _tables[property.table].Set(hash, value);
        }

        public bool Contains(int hash) {
            return _propertyMap.TryGetValue(hash, out var property) && _tables[property.table].Contains(hash);
        }

        public static int StringToHash(string name) {
            return name.GetHashCode();
        }

#if UNITY_EDITOR
        private Blackboard2 _overridenBlackboard;

        public bool OverrideBlackboard(Blackboard2 blackboard) {
            _overridenBlackboard = blackboard;
            bool changed = false;

            for (int i = _propertyList.Count - 1; i >= 0; i--) {
                int hash = _propertyList[i];
                var property = _propertyMap[hash];

                if (!blackboard._propertyMap.TryGetValue(hash, out var p) || p.type.ToType() is not {} t) {
                    _propertyList.RemoveAt(i);
                    _propertyMap.Remove(hash);

                    changed |= TryRemoveValue(property.table, hash);
                    continue;
                }

                if (p.type == property.type) continue;

                changed |= TryRemoveValue(property.table, hash);

                var tableType = BlackboardTableUtils.GetBlackboardTableType(t);
                int table = GetOrCreateTable(tableType);

                property.type = p.type;
                property.table = table;
                _propertyMap[hash] = property;

                blackboard.TryGetValue(p.table, hash, out object v);
                changed |= TrySetValue(table, hash, v);
            }

            for (int i = 0; i < blackboard._propertyList.Count; i++) {
                int hash = blackboard._propertyList[i];
                var p = blackboard._propertyMap[hash];
                if (p.type.ToType() is not {} t) continue;

                if (!_propertyMap.ContainsKey(hash)) {
                    var tableType = BlackboardTableUtils.GetBlackboardTableType(t);
                    int table = GetOrCreateTable(tableType);
                    int pTable = p.table;

                    p.table = table;
                    _propertyMap.Add(hash, p);
                    _propertyList.Add(hash);

                    blackboard.TryGetValue(pTable, hash, out object v);
                    changed |= TrySetValue(table, hash, v);
                }

                changed |= TrySetPropertyIndex(hash, i);
            }

            return changed;
        }

        public bool TryAddProperty(string name, Type type) {
            var tableType = BlackboardTableUtils.GetBlackboardTableType(type);
            if (tableType == null) return false;

            name = ValidateName(name);
            int hash = StringToHash(name);
            if (_propertyMap.ContainsKey(hash)) return false;

            int table = GetOrCreateTable(tableType);
            if (table < 0) return false;

            var property = new BlackboardProperty2 {
                name = name,
                type = new SerializedType(type),
                table = table,
            };

            TrySetValue(table, hash, default);

            _propertyMap.Add(hash, property);
            _propertyList.Add(hash);
            return true;
        }

        public bool TryGetProperty(int hash, out BlackboardProperty2 property) {
            return _propertyMap.TryGetValue(hash, out property);
        }

        public bool TryGetPropertyValue(int hash, out object value) {
            if (!_propertyMap.TryGetValue(hash, out var property)) {
                value = default;
                return false;
            }

            return TryGetValue(property.table, hash, out value);
        }

        public bool TrySetPropertyValue(int hash, object value) {
            return _propertyMap.TryGetValue(hash, out var property) &&
                   TrySetValue(property.table, hash, property.type.ToType() == null ? default : value);
        }

        public bool TryResetPropertyValues() {
            bool changed = false;
            for (int i = 0; i < _propertyList.Count; i++) {
                changed |= TryResetPropertyValue(_propertyList[i]);
            }

            return changed;
        }

        public bool TryResetPropertyValue(int hash) {
            if (!_propertyMap.TryGetValue(hash, out var property) || property.type.ToType() is not {} t) return false;

            object value = _overridenBlackboard != null &&
                           _overridenBlackboard.TryGetPropertyValue(hash, out object v) &&
                           v != null &&
                           t.IsInstanceOfType(v)
                ? v : default;

            return TrySetValue(property.table, hash, value);
        }

        public bool TrySetPropertyName(int hash, string newName) {
            if (!_propertyMap.TryGetValue(hash, out var property)) return false;

            newName = ValidateName(newName);
            int newHash = StringToHash(newName);
            if (_propertyMap.ContainsKey(newHash)) return false;

            property.name = newName;

            _propertyMap.Remove(hash);
            _propertyMap[newHash] = property;

            for (int i = 0; i < _propertyList.Count; i++) {
                if (_propertyList[i] != hash) continue;

                _propertyList[i] = newHash;
                break;
            }

            int table = property.table;

            TryGetValue(table, hash, out object value);
            TryRemoveValue(table, hash);
            TrySetValue(table, newHash, value);

            return true;
        }

        public bool TrySetPropertyIndex(int hash, int newIndex) {
            if (newIndex < 0) return false;

            int oldIndex = -1;
            for (int i = 0; i < _propertyList.Count; i++) {
                if (_propertyList[i] != hash) continue;

                oldIndex = i;
                break;
            }

            if (oldIndex < 0 || oldIndex == newIndex) return false;

            if (newIndex >= _propertyList.Count) {
                _propertyList.RemoveAt(oldIndex);
                _propertyList.Add(hash);
                return true;
            }

            _propertyList[oldIndex] = _propertyList[newIndex];
            _propertyList[newIndex] = hash;

            return true;
        }

        public void RemoveProperty(int hash) {
            if (_propertyMap.TryGetValue(hash, out var property)) _propertyMap.Remove(hash);

            for (int i = _propertyList.Count - 1; i >= 0; i--) {
                if (_propertyList[i] != hash) continue;

                _propertyList.RemoveAt(i);
                break;
            }

            TryRemoveValue(property.table, hash);
        }

        public string GetSerializedPropertyPath(int hash) {
            if (!_propertyMap.TryGetValue(hash, out var property)) return null;
            if (property.table < 0 || property.table >= _tablesHead) return null;

            string tableLocalPath = _tables[property.table].GetSerializedPropertyPath(hash);
            if (tableLocalPath == null) return null;

            return $"{nameof(_tables)}._nodes.Array.data[{_tables.IndexOf(property.table)}].{tableLocalPath}";
        }

        private bool TryGetValue(int table, int hash, out object value) {
            return _tables[table].TryGetValue(hash, out value);
        }

        private bool TrySetValue(int table, int hash, object value) {
            return _tables[table].TrySetValue(hash, value);
        }

        private bool TryRemoveValue(int table, int hash) {
            var t = _tables[table];
            bool removed = t.RemoveValue(hash);
            if (t.Count <= 0) _tables.Remove(table);

            return removed;
        }

        private int GetOrCreateTable(Type tableType) {
            for (int i = 0; i < _tables.Count; i++) {
                if (_tables[i] is {} t && t.GetType() == tableType) return i;
            }

            var table = (IBlackboardTable) Activator.CreateInstance(tableType);
            int id = _tablesHead++;
            _tables[id] = table;

            return id;
        }

        private string ValidateName(string name) {
            int hash = StringToHash(name);
            if (!_propertyMap.ContainsKey(hash)) return name;

            int count = 1;
            string pattern = $@"{name} \([0-9]+\)";

            for (int i = 0; i < _propertyList.Count; i++) {
                if (_propertyMap[_propertyList[i]].name.HasRegexPattern(pattern)) count++;
            }

            return $"{name} ({count})";
        }
#endif
    }

}
