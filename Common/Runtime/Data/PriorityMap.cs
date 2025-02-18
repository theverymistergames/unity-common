using System.Collections.Generic;

namespace MisterGames.Common.Data {
    
    public sealed class PriorityMap<K, V> : IComparer<K> {

        public V this[K key] { get => _map[key]; set => _map[key] = value; } 
        public int Count => _map.Count;
        
        private readonly SortedDictionary<K, V> _map;
        private readonly Dictionary<K, int> _priorityMap;
        private V _resultCache;

        public PriorityMap() {
            _map = new SortedDictionary<K, V>(this);
            _priorityMap = new Dictionary<K, int>();
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
            return _map.GetValueOrDefault(key, defaultValue);
        }

        public bool TryGetValue(K key, out V value) {
            return _map.TryGetValue(key, out value);
        }
        
        public void Set(K key, V value, int priority) {
            _priorityMap[key] = priority;
            _map[key] = value;
            _resultCache = GetResult();
        }

        public void Remove(K key) {
            _priorityMap.Remove(key);
            _map.Remove(key);
            _resultCache = GetResult();
        }

        public void Clear() {
            _priorityMap.Clear();
            _map.Clear();
            _resultCache = default;
        }

        int IComparer<K>.Compare(K x, K y) {
            return _priorityMap.GetValueOrDefault(x).CompareTo(_priorityMap.GetValueOrDefault(y));
        }

        private V GetResult() {
            foreach (var v in _map.Values) {
                return v;
            }

            return default;
        }
    }
    
}