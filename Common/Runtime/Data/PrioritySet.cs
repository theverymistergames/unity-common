using System.Collections.Generic;

namespace MisterGames.Common.Data {
    
    public sealed class PrioritySet<T> : IComparer<T> {
        
        public int Count => _map.Count;
        
        private readonly Dictionary<T, int> _map = new();
        private readonly List<T> _sortedKeys = new();
        private T _resultCache;
        
        public bool TryGetResult(out T value) {
            if (_map.Count > 0) {
                value = _resultCache;
                return true;
            }
            
            value = default;
            return false;
        }
        
        public T GetResultOrDefault(T defaultValue = default) {
            return _map.Count > 0 ? _resultCache : defaultValue;
        }
        
        public void Set(T key, int priority) {
            if (_map.TryAdd(key, priority)) _sortedKeys.Add(key);
            else _map[key] = priority;

            _sortedKeys.Sort(this);
            _resultCache = GetResult(); 
        }

        public void Remove(T key) {
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

        int IComparer<T>.Compare(T x, T y) {
            return _map.GetValueOrDefault(x).CompareTo(_map.GetValueOrDefault(y));
        }

        private T GetResult() {
            return _sortedKeys.Count > 0 ? _sortedKeys[0] : default;
        }
    }
    
}