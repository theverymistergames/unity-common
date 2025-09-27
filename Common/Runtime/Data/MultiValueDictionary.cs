using System;
using System.Collections.Generic;
using System.Text;
using MisterGames.Common.Maths;

namespace MisterGames.Common.Data
{
    
    public sealed class MultiValueDictionary<K, V> {
        
        public int Count => _valueMap.Count;
        public Dictionary<K, int>.KeyCollection Keys => _countMap.Keys;
        public Dictionary<(K, int), V>.ValueCollection Values => _valueMap.Values;
        
        private readonly Dictionary<K, int> _countMap;
        private readonly Dictionary<(K, int), V> _valueMap;
        private int _version;

        public MultiValueDictionary(int capacity = 0) {
            _countMap = new Dictionary<K, int>(capacity);
            _valueMap = new Dictionary<(K, int), V>(capacity);
        }

        public bool ContainsKey(K key) {
            return _countMap.ContainsKey(key);
        }

        public int GetCount(K key) {
            return _countMap.GetValueOrDefault(key);
        }
        
        public V GetFirstValue(K key) {
            if (!_valueMap.TryGetValue((key, 0), out var value)) {
                throw new ArgumentException($"No entry with key {key} was found");
            }

            return value;
        }
        
        public bool TryGetFirstValue(K key, out V value) {
            return _valueMap.TryGetValue((key, 0), out value);
        }

        public V GetValueAt(K key, int index) {
            if (!_valueMap.TryGetValue((key, index), out var value)) {
                throw new ArgumentException($"No entry with key {key} and index {index} was found");
            }

            return value;
        }

        public bool TryGetValueAt(K key, int index, out V value) {
            return _valueMap.TryGetValue((key, index), out value);
        }
        
        public int GetValueIndex(K key, V value) {
            int count = _countMap.GetValueOrDefault(key);
            
            for (int i = 0; i < count; i++) {
                if (EqualityComparer<V>.Default.Equals(value, _valueMap[(key, i)])) {
                    return i;
                }
            }
            
            return -1;
        }

        public bool TryGetValueIndex(K key, V value, out int index) {
            index = GetValueIndex(key, value);
            return index >= 0;
        }
        
        public bool ContainsValue(K key, V value) {
            return GetValueIndex(key, value) >= 0;
        }
        
        public int AddValue(K key, V value) {
            int index = _countMap.GetValueOrDefault(key);
            
            _countMap[key] = index + 1;
            _valueMap[(key, index)] = value;
            _version.IncrementUncheckedRef();

            return index;
        }

        public bool TrySetValueAt(K key, int index, V value) {
            int count = _countMap.GetValueOrDefault(key);
            if (index < 0 || index > count) return false;
            
            if (index == count) _countMap[key] = count + 1;
            
            _valueMap[(key, index)] = value;
            _version.IncrementUncheckedRef();
            
            return true;
        }
        
        public void SetValueAt(K key, int index, V value) {
            int count = _countMap.GetValueOrDefault(key);
            if (index < 0 || index > count) {
                throw new ArgumentException($"Trying to set value {value} for key {key} at index {index} that is out of range, key count is {count}.");
            }
            
            if (index == count) _countMap[key] = count + 1;
            
            _valueMap[(key, index)] = value;
            _version.IncrementUncheckedRef();
        }
        
        public bool RemoveValue(K key, V value) {
            int index = GetValueIndex(key, value);
            return index >= 0 && RemoveValueAt(key, index);
        }
        
        public bool RemoveValueAt(K key, int index) {
            return RemoveValueAt(key, index, out _);
        }

        public bool RemoveValueAt(K key, int index, out V value) {
            int count = _countMap.GetValueOrDefault(key);
            
            if (index < 0 || index > count - 1) {
                value = default;
                return false;
            }
            
            _valueMap.Remove((key, index), out value);

            if (count > 1) {
                if (index < count - 1) {
                    _valueMap[(key, index)] = _valueMap[(key, count - 1)];
                }
                
                _countMap[key] = count - 1;
            }
            else {
                _countMap.Remove(key);
            }
            
            _version.IncrementUncheckedRef();
            return true;
        }
        
        public ValueCollection GetValues(K key) {
            return new ValueCollection(this, key);
        }

        public void SetValues(K key, IReadOnlyCollection<V> values) {
            RemoveValues(key);
            
            int count = values?.Count ?? 0;
            if (count == 0) return;
            
            _countMap[key] = count;
            
            int i = 0;
            foreach (var value in values!) {
                _valueMap[(key, i++)] = value;
            }
            
            _version.IncrementUncheckedRef();
        }
        
        public void AddValues(K key, IReadOnlyCollection<V> values) {
            int newCount = values?.Count ?? 0;
            if (newCount == 0) return;
            
            int existentCount = _countMap.GetValueOrDefault(key);
            _countMap[key] = existentCount + newCount;
            
            int i = existentCount;
            foreach (var value in values!) {
                _valueMap[(key, i++)] = value;
            }
            
            _version.IncrementUncheckedRef();
        }

        public int RemoveValues(K key) {
            int count = _countMap.GetValueOrDefault(key);
            for (int i = 0; i < count; i++) {
                _valueMap.Remove((key, i));
            }

            _countMap.Remove(key);
            _version.IncrementUncheckedRef();
            
            return count;
        }

        public void Clear() {
            _countMap.Clear();
            _valueMap.Clear();
            _version.IncrementUncheckedRef();
        }

        public override string ToString() {
            return $"{typeof(MultiValueDictionary<K, V>).Name}<{nameof(K)}, {nameof(V)}>(keys {_countMap.Count}, values {_valueMap.Count}):\n{GetStateString()}";
        }

        private string GetStateString() {
            var sb = new StringBuilder();

            sb.AppendLine("Keys:");
            
            foreach ((var key, int count) in _countMap) {
                sb.AppendLine($"- Key {key} (count {count})");
            }
            
            sb.AppendLine("Values:");
            
            foreach (var (key, value) in _valueMap) {
                sb.AppendLine($"- Key {key.Item1}[{key.Item2}], value {value}");
            }
            
            return sb.ToString();
        }

        public struct ValueCollection {
            public V Current => _source.GetValueAt(_key, _index);
            
            private readonly MultiValueDictionary<K, V> _source;
            private readonly K _key;
            private readonly int _version;
            private readonly int _count;
            private int _index;

            public ValueCollection(MultiValueDictionary<K, V> source, K key) {
                _source = source;
                _key = key;
                _index = -1;
                _count = _source.GetCount(_key);
                _version = source._version;
            }

            public bool MoveNext() {
                return ++_index < _count && _version == _source._version;
            }

            public ValueCollection GetEnumerator() {
                return this;
            }
        }
    }
    
}