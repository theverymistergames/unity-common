using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Common.Data {

    [Serializable]
    public sealed class DictionaryList<K, V> {

        public int Count => _entries.Count;
        public readonly KeyAccessor Keys;
        public readonly ValueAccessor Values;

        [SerializeField]
        private List<Entry> _entries;

        public DictionaryList(int capacity = 0) {
            _entries = new List<Entry>(capacity);

            Keys = new KeyAccessor(this);
            Values = new ValueAccessor(this);
        }

        public void Add(K key, V value) {
            _entries.Add(new Entry(key, value));
        }

        public void Insert(int index, K key, V value) {
            _entries.Insert(index, new Entry(key, value));
        }

        public void RemoveAt(int index) {
            _entries.RemoveAt(index);
        }

        public void Clear() {
            _entries.Clear();
        }

        public readonly struct KeyAccessor {
            public K this[int index] => _owner._entries[index].key;
            public int IndexOf(K key) => _owner._entries.IndexOf(new Entry(key));
            public int IndexOf(K key, int index) => _owner._entries.IndexOf(new Entry(key), index);
            public int LastIndexOf(K key) => _owner._entries.LastIndexOf(new Entry(key));
            public int LastIndexOf(K key, int index) => _owner._entries.LastIndexOf(new Entry(key), index);

            private readonly DictionaryList<K, V> _owner;
            public KeyAccessor(DictionaryList<K, V> owner) => _owner = owner;
        }

        public readonly struct ValueAccessor {
            public V this[int index] {
                get => _owner._entries[index].value;
                set => _owner._entries[index] = new Entry(_owner._entries[index].key, value);
            }

            private readonly DictionaryList<K, V> _owner;
            public ValueAccessor(DictionaryList<K, V> owner) => _owner = owner;
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
