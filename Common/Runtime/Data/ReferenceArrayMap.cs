using System;
using System.Collections.Generic;
using System.Text;
using MisterGames.Common.Lists;
using UnityEngine;

namespace MisterGames.Common.Data {

    [Serializable]
    public sealed class ReferenceArrayMap<K, V> where V : class {

        [SerializeField] private Node[] _nodes;
        [SerializeField] private SerializedDictionary<K, int> _indexMap;
        [SerializeField] private int _head;

        public int Count => _indexMap.Count;

        public Dictionary<K, int>.KeyCollection Keys => _indexMap.Keys;

        public V this[K key] {
            get => GetValue(key);
            set => SetValue(key, value);
        }

        private readonly List<int> _freeIndices;

        [Serializable]
        private struct Node {

            public K key;
            [SerializeReference] public V value;
            public bool isDisposed;

            public override string ToString() {
                return $"{nameof(Node)}({(isDisposed ? "disposed" : $"key {key}, value {value}")})";
            }
        }

        public ReferenceArrayMap(int capacity = 0) {
            _nodes = new Node[capacity];
            _indexMap = new SerializedDictionary<K, int>(capacity);
            _freeIndices = new List<int>();
        }

        public bool TryGetValue(K key, out V value) {
            if (_indexMap.TryGetValue(key, out int index)) {
                ref var node = ref _nodes[index];
                value = node.value;
                return true;
            }

            value = default;
            return false;
        }

        public V GetValue(K key) {
            if (!_indexMap.TryGetValue(key, out int index)) {
                throw new KeyNotFoundException($"{nameof(ArrayMap<K, V>)}: key {key} not found");
            }

            ref var node = ref _nodes[index];
            return node.value;
        }

        public void SetValue(K key, V value) {
            if (_indexMap.TryGetValue(key, out int index)) {
                ref var node = ref _nodes[index];
                node.value = value;
                return;
            }

            Add(key, value);
        }

        public void Add(K key, V value) {
            if (_indexMap.TryGetValue(key, out int index)) {
                throw new InvalidOperationException($"{nameof(ArrayMap<K, V>)}: key {key} was already added");
            }

            _indexMap[key] = AllocateNode(key, value);
        }

        public bool Remove(K key) {
            if (!_indexMap.TryGetValue(key, out int index)) return false;

            if (index == _head - 1) _head--;
            else _freeIndices.Add(index);

            ref var node = ref _nodes[index];
            node.isDisposed = true;

            _indexMap.Remove(key);

            ApplyDefragmentationIfNecessary();
            return true;
        }

        public bool ContainsKey(K key) {
            return _indexMap.ContainsKey(key);
        }

        public int IndexOf(K key) {
            return _indexMap.TryGetValue(key, out int index) ? index : -1;
        }

        public void Clear() {
            _nodes = Array.Empty<Node>();
            _indexMap.Clear();
            _freeIndices.Clear();
            _head = 0;
        }

        private int AllocateNode(K key, V value) {
            int index = -1;

            for (int i = _freeIndices.Count - 1; i >= 0; i--) {
                index = _freeIndices[i];
                _freeIndices.RemoveAt(i);

                if (index < _head - 1) break;
                if (index == _head - 1) _head--;
            }

            if (index < 0 || index >= _head) {
                index = _head++;
                ArrayExtensions.EnsureCapacity(ref _nodes, _head);
            }

            ref var node = ref _nodes[index];

            node.key = key;
            node.value = value;
            node.isDisposed = false;

            return index;
        }

        private void ApplyDefragmentationIfNecessary() {
            int count = Count;
            if (count > _nodes.Length * 0.5f) return;

            int j = _freeIndices.Count - 1;

            for (int i = _head - 1; i >= count; i--) {
                ref var node = ref _nodes[i];
                if (node.isDisposed) continue;

                int freeIndex = -1;
                while (j >= 0) {
                    freeIndex = _freeIndices[j--];
                    if (freeIndex < count) break;
                }

                if (freeIndex < 0 || freeIndex >= count) break;

                _nodes[freeIndex] = node;
                _indexMap[node.key] = freeIndex;
            }

            _head = count;
            _freeIndices.Clear();
            Array.Resize(ref _nodes, _head);
        }

        public override string ToString() {
            var sb = new StringBuilder();
            sb.AppendLine($"{nameof(ArrayMap<K, V>)}(count {Count})");

            for (int i = 0; i < _nodes.Length; i++) {
                ref var node = ref _nodes[i];
                if (node.isDisposed) continue;

                sb.AppendLine($"[{i}] :: {node}");
            }

            return sb.ToString();
        }
    }

}
