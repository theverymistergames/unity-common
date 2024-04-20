using System;
using System.Collections.Generic;

namespace MisterGames.Common.Data
{
    
    public sealed class MultiValueDictionary<K, V> {
        
        public int Count => _valueMap.Count;
        public IReadOnlyCollection<V> Values => _valueMap.Values;
        
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

        public V GetValue(K key, int index) {
            if (!_valueMap.TryGetValue((key, index), out var value)) {
                throw new ArgumentException($"No entry with key {key} and index {index} was found");
            }

            return value;
        }

        public bool TryGetValue(K key, int index, out V value) {
            return _valueMap.TryGetValue((key, index), out value);
        }

        public int AddValue(K key, V value) {
            int index = _countMap.GetValueOrDefault(key);
            
            _valueMap[(key, index)] = value;
            _countMap[key] = index + 1;
            _version++;
            
            return index;
        }

        public void SetValue(K key, int index, V value) {
            if (!_valueMap.ContainsKey((key, index))) {
                throw new ArgumentException($"No entry with key {key} and index {index} was found");
            }
            
            _valueMap[(key, index)] = value;
            _version++;
        }

        public bool TrySetValue(K key, int index, V value) {
            if (!_valueMap.ContainsKey((key, index))) return false;
            
            _valueMap[(key, index)] = value;
            _version++;
            
            return true;
        }

        public bool RemoveValue(K key, int index) {
            int count = _countMap.GetValueOrDefault(key);
            if (index < 0 || index > count - 1) return false;
            
            _valueMap.Remove((key, index));

            if (count > 0) _countMap[key] = count - 1;
            else _countMap.Remove(key);

            for (int i = index + 1; i < count; i++) {
                _valueMap[(key, i - 1)] = _valueMap[(key, i)];
            }
            
            _version++;
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
            foreach (var value in values) {
                _valueMap[(key, i++)] = value;
            }
            
            _version++;
        }
        
        public void AddValues(K key, IReadOnlyCollection<V> values) {
            int newCount = values?.Count ?? 0;
            if (newCount == 0) return;
            
            int existentCount = _countMap.GetValueOrDefault(key);
            _countMap[key] = existentCount + newCount;
            
            int i = existentCount;
            foreach (var value in values) {
                _valueMap[(key, i++)] = value;
            }
            
            _version++;
        }

        public int RemoveValues(K key) {
            int count = _countMap.GetValueOrDefault(key);
            for (int i = 0; i < count; i++) {
                _valueMap.Remove((key, i));
            }

            _countMap.Remove(key);
            _version++;
            
            return count;
        }

        public void Clear() {
            _countMap.Clear();
            _valueMap.Clear();
            _version++;
        }

        public struct ValueCollection {
            public V Current => _source.GetValue(_key, _index);
            
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