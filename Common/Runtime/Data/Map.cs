using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Common.Data {

    [Serializable]
    public class Map<K, V> {
        
        [Serializable]
        private struct Tuple {
            public K Key;
            public V Value;
        }
        
        [SerializeField] private List<Tuple> _tuples = new List<Tuple>();

        public Optional<V> Get(K key) {
            for (int i = 0; i < _tuples.Count; i++) {
                var tuple = _tuples[i];
                if (tuple.Key.GetHashCode() == key.GetHashCode()) {
                    return Optional<V>.WithValue(tuple.Value);
                }
            }
            return Optional<V>.Empty();
        }

        public void Set(K key, V value) {
            for (int i = 0; i < _tuples.Count; i++) {
                var tuple = _tuples[i];
                if (tuple.Key.GetHashCode() != key.GetHashCode()) continue;
                
                tuple.Value = value;
                _tuples[i] = tuple;
                return;
            }
            
            _tuples.Add(new Tuple { Key = key, Value = value });
        }

        public IReadOnlyList<K> GetKeys() {
            int count = _tuples.Count;
            var keys = new K[count];
            
            for (int i = 0; i < count; i++) {
                keys[i] = _tuples[i].Key;
            }

            return keys;
        }

        public void Clear() {
            _tuples.Clear();
        }
    }

}