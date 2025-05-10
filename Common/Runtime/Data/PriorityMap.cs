using System.Collections.Generic;

namespace MisterGames.Common.Data {
    
    public sealed class PriorityMap<K, V> : IComparer<K> {
        
        public int Count => _map.Count;
        
        public V this[K key] {
            get => _map[key].value; 
            set => _map[key] = _map.GetValueOrDefault(key).WithValue(value);
        }
        
        private readonly Dictionary<K, Entry> _map = new();
        private readonly List<K> _sortedKeys = new();
        private V _resultCache;

        private readonly struct Entry {
            
            public readonly V value;
            public readonly int priority;
            
            public Entry(V value, int priority = 0) {
                this.value = value;
                this.priority = priority;
            }

            public Entry WithValue(V value) => new(value, priority);
        }
        
        public bool TryGetResult(out V value) {
            if (_map.Count > 0) {
                value = _resultCache;
                return true;
            }
            
            value = default;
            return false;
        }
        
        public V GetResultOrDefault(V defaultValue = default) {
            return _map.Count > 0 ? _resultCache : defaultValue;
        }
        
        public V GetValueOrDefault(K key, V defaultValue = default) {
            return _map.TryGetValue(key, out var entry) ? entry.value : defaultValue;
        }

        public bool TryGetValue(K key, out V value) {
            if (_map.TryGetValue(key, out var entry)) {
                value = entry.value;
                return true;
            }
            
            value = default;
            return false;
        }
        
        public void Set(K key, V value, int priority) {
            var entry = new Entry(value, priority);

            if (_map.TryAdd(key, entry)) _sortedKeys.Add(key);
            else _map[key] = entry;

            _sortedKeys.Sort(this);
            _resultCache = GetResult(); 
        }

        public void Remove(K key) {
            if (!_map.Remove(key)) return;
            
            _sortedKeys.Remove(key);
            _sortedKeys.Sort(this);

            _resultCache = GetResult();
        }

        public void Clear() {
            _sortedKeys.Clear();
            _map.Clear();
            _resultCache = default;
        }

        int IComparer<K>.Compare(K x, K y) {
            return _map.GetValueOrDefault(x).priority.CompareTo(_map.GetValueOrDefault(y).priority);
        }

        private V GetResult() {
            return _sortedKeys.Count > 0 ? _map[_sortedKeys[0]].value : default;
        }
    }
    
}