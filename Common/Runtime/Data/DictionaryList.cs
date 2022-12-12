using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Common.Data {

    [Serializable]
    public sealed class DictionaryList<K, V> {

        public int Count => _entries.Count;

        public V this[int index] {
            get => _entries[index].value;
            set => _entries[index] = new Entry(_entries[index].key, value);
        }

        [SerializeField]
        private List<Entry> _entries;

        public DictionaryList(int capacity = 0) {
            _entries = new List<Entry>(capacity);
        }

        public void Add(K key, V value) {
            _entries.Add(new Entry(key, value));
        }

        public void RemoveAt(int index) {
            _entries.RemoveAt(index);
        }

        public int IndexOf(K key) {
            var keyEntry = new Entry(key);
            return _entries.IndexOf(keyEntry);
        }

        public int LastIndexOf(K key) {
            var keyEntry = new Entry(key);
            return _entries.LastIndexOf(keyEntry);
        }

        public void Clear() {
            _entries.Clear();
        }

        [Serializable]
        private struct Entry : IEquatable<Entry> {

            public K key;
            public V value;

            public Entry(K key, V value = default) {
                this.key = key;
                this.value = value;
            }

            public bool Equals(Entry other) {
                return EqualityComparer<K>.Default.Equals(key, other.key);
            }

            public override bool Equals(object obj) {
                return obj is Entry other && Equals(other);
            }

            public override int GetHashCode() {
                return EqualityComparer<K>.Default.GetHashCode(key);
            }

            public static bool operator ==(Entry left, Entry right) {
                return left.Equals(right);
            }

            public static bool operator !=(Entry left, Entry right) {
                return !left.Equals(right);
            }
        }
    }

}
