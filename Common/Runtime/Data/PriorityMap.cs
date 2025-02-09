using System;
using System.Collections.Generic;

namespace MisterGames.Common.Data {
    
    public sealed class PriorityMap<K, V> : IComparer<PriorityMap<K, V>.KeyData> {

        public V this[K key] { get => _map[new KeyData(key)]; set => _map[new KeyData(key)] = value; } 
        public int Count => _map.Count;
        
        private readonly SortedDictionary<KeyData, V> _map;
        private V _resultCache;
        
        private readonly struct KeyData : IEquatable<KeyData> {

            public readonly K key;
            public readonly int priority;
            
            public KeyData(K key, int priority = 0) {
                this.key = key;
                this.priority = priority;
            }
            
            public bool Equals(KeyData other) => EqualityComparer<K>.Default.Equals(key, other.key);
            public override bool Equals(object obj) => obj is KeyData other && Equals(other);
            public override int GetHashCode() => EqualityComparer<K>.Default.GetHashCode(key);
            public static bool operator ==(KeyData left, KeyData right) => left.Equals(right);
            public static bool operator !=(KeyData left, KeyData right) => !left.Equals(right);
        }

        public PriorityMap() {
            _map = new SortedDictionary<KeyData, V>(this);
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
            return _map.GetValueOrDefault(new KeyData(key), defaultValue);
        }

        public bool TryGetValue(K key, out V value) {
            return _map.TryGetValue(new KeyData(key), out value);
        }
        
        public void Set(K key, V value, int priority) {
            _map[new KeyData(key, priority)] = value;
            _resultCache = GetResultOrDefault();
        }

        public void Remove(K key) {
            _map.Remove(new KeyData(key));
            _resultCache = GetResultOrDefault();
        }

        public void Clear() {
            _map.Clear();
            _resultCache = default;
        }

        int IComparer<KeyData>.Compare(KeyData x, KeyData y) {
            return x.priority.CompareTo(y.priority);
        }
    }
    
}