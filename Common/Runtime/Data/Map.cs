using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Common.Data {

    [Serializable]
    public sealed class Map<K, V> {

        [SerializeField] private List<Tuple> _tuples = new List<Tuple>();

        public int Count => _tuples.Count;

        public V this[K key] {
            get {
                int index = IndexOf(key);
                if (index < 0) return default;

                return _tuples[index].value;
            }
            set {
                int index = IndexOf(key);
                if (index < 0) return;

                _tuples[index] = new Tuple(key, value);
            }
        }

        public void Remove(K key) {
            int index = IndexOf(key);
            if (index < 0) return;

            _tuples.RemoveAt(index);
        }

        public K GetKeyAt(int index) {
            return _tuples[index].key;
        }

        public V GetValueAt(int index) {
            return _tuples[index].value;
        }

        public void RemoveAt(int index) {
            _tuples.RemoveAt(index);
        }

        public int IndexOf(K key) {
            return _tuples.IndexOf(new Tuple(key));
        }

        public int LastIndexOf(K key) {
            return _tuples.LastIndexOf(new Tuple(key));
        }

        public void Clear() {
            _tuples.Clear();
        }

        [Serializable]
        private struct Tuple : IEquatable<Tuple> {

            public K key;
            public V value;

            public Tuple(K key, V value = default) {
                this.key = key;
                this.value = value;
            }

            public bool Equals(Tuple other) {
                return EqualityComparer<K>.Default.Equals(key, other.key);
            }

            public override bool Equals(object obj) {
                return obj is Tuple other && Equals(other);
            }

            public override int GetHashCode() {
                return EqualityComparer<K>.Default.GetHashCode(key);
            }

            public static bool operator ==(Tuple left, Tuple right) {
                return left.Equals(right);
            }

            public static bool operator !=(Tuple left, Tuple right) {
                return !left.Equals(right);
            }
        }
    }

}
