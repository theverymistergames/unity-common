using System.Collections.Generic;

namespace MisterGames.Common.Data {

    public sealed class DictionaryList<K, V> { // todo implement collection interfaces

        public int Count => _keys.Count;
        public IReadOnlyList<K> Keys => _keys;
        public IReadOnlyList<V> Values => _values;

        public V this[int index] {
            get => _values[index];
            set => _values[index] = value;
        }

        private readonly List<K> _keys;
        private readonly List<V> _values;

        public DictionaryList(int capacity = 0) {
            _keys = new List<K>(capacity);
            _values = new List<V>(capacity);
        }

        public void Add(K key, V data) {
            _keys.Add(key);
            _values.Add(data);
        }

        public void RemoveAt(int index) {
            _keys.RemoveAt(index);
            _values.RemoveAt(index);
        }

        public int IndexOf(K key) {
            return _keys.IndexOf(key);
        }

        public int LastIndexOf(K key) {
            return _keys.LastIndexOf(key);
        }

        public void Clear() {
            _keys.Clear();
            _values.Clear();
        }
    }
}
