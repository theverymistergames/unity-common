using System;
using System.Collections.Generic;
using System.Text;
using MisterGames.Common.Lists;
using UnityEngine;

namespace MisterGames.Common.Data {

    [Serializable]
    public sealed class SerializedReferenceMap<K, V>
        where K : struct
        where V : class 
    {
        [SerializeField] private Entry[] _entries;
        [SerializeField] private SerializedDictionary<K, int> _indexMap;

        [Serializable]
        public struct Entry {
            public K key;
            [SerializeReference] public V value;

            public override string ToString() {
                return $"Entry(key {key}, value {value})";
            }
        }

        public int Count => _indexMap.Count;
        public Entry[] Entries => _entries;

        private readonly Queue<int> _freeIndices = new Queue<int>();
        
        public SerializedReferenceMap(int capacity = 0) {
            _entries = new Entry[capacity];
            _indexMap = new SerializedDictionary<K, int>(capacity);
        }

        public V Get(K key) {
            if (!_indexMap.TryGetValue(key, out int index)) return null;
            
            ref var entry = ref _entries[index];
            return entry.value;
        }

        public int IndexOf(K key) {
            return _indexMap.TryGetValue(key, out int index) ? index : -1;
        }

        public void Add(K key, V value) {
            int index = _freeIndices.TryDequeue(out int freeIndex) ? freeIndex : _indexMap.Count;

            _indexMap.Add(key, index);
            ArrayExtensions.EnsureCapacity(ref _entries, index + 1);

            ref var entry = ref _entries[index];

            entry.key = key;
            entry.value = value;
        }

        public void Remove(K key) {
            if (!_indexMap.TryGetValue(key, out int index)) return;
            
            ref var entry = ref _entries[index];
            
            entry.value = null;

            _indexMap.Remove(key);
            _freeIndices.Enqueue(index);

            if (_freeIndices.Count < _indexMap.Count * 0.5f) return;

            OptimizeLayout();
        }

        public bool ContainsKey(K key) {
            return _indexMap.ContainsKey(key);
        }

        public void Clear() {
            _entries = null;
            _indexMap.Clear();
            _freeIndices.Clear();
        }
        
        public void OptimizeLayout() {
            int offset = 0;

            for (int i = 0; i < _entries.Length; i++) {
                ref var entryA = ref _entries[i];

                if (entryA.value == null) {
                    offset++;
                    continue;
                }

                if (offset == 0) continue;

                int newIndex = i - offset;
                ref var entryB = ref _entries[newIndex];

                entryB = entryA;
                _indexMap[entryA.key] = newIndex;
            }

            Array.Resize(ref _entries, _indexMap.Count);

            _freeIndices.Clear();
        }

        public override string ToString() {
            var sb = new StringBuilder();
            sb.AppendLine($"{nameof(SerializedReferenceMap<K, V>)}(values {_indexMap.Count}/{_entries.Length}):");

            for (int i = 0; i < _entries.Length; i++) {
                sb.AppendLine($"[{i}] :: {_entries[i]}");
            }

            return sb.ToString();
        }
    }

}
