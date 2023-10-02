using System;
using System.Collections.Generic;
using MisterGames.Common.Lists;
using UnityEngine;

namespace MisterGames.Common.Data {

    [Serializable]
    public sealed class LinearTreeMap<K, V> {

#region DATA

        [SerializeField] private Entry[] _entries;
        [SerializeField] private SerializedDictionary<K, int> _keyToIndexMap;
        [SerializeField] private SerializedDictionary<int, K> _indexToKeyMap;
        [SerializeField] private List<int> _freeIndices;
        [SerializeField] private int _head;
        [SerializeField] private int _aliveCount;

        [Serializable]
        private struct Entry {

            public V value;
            public int root;
            public int next;
            public int child;

            public void Reset() {
                value = default;
                root = -1;
                next = -1;
                child = -1;
            }

            public void Dispose() {
                value = default;
                root = -2;
                next = -1;
                child = -1;
            }

            public bool IsDisposed() {
                return root < -1;
            }

            public override string ToString() {
                return $"{nameof(Entry)}(root {root}, next {next}, child {child}, value {value})";
            }
        }

        public readonly struct Node : IEquatable<Node> {

            public readonly int index;
            public readonly int next;
            public readonly int child;

            public Node(int index, int next, int child) {
                this.index = index;
                this.next = next;
                this.child = child;
            }

            public bool Equals(Node other) {
                return index == other.index && next == other.next && child == other.child;
            }

            public override bool Equals(object obj) {
                return obj is Node other && Equals(other);
            }

            public override int GetHashCode() {
                return HashCode.Combine(index, next, child);
            }

            public static bool operator ==(Node left, Node right) {
                return left.Equals(right);
            }

            public static bool operator !=(Node left, Node right) {
                return !left.Equals(right);
            }

            public override string ToString() {
                return $"Node(index {index}, next {next}, child {child})";
            }
        }

        private static Node InvalidNode => new Node(-1, -1, -1);

#endregion

#region CONSTRUCTORS

        public LinearTreeMap(int capacity = 0) {
            _entries = capacity > 0 ? new Entry[capacity] : Array.Empty<Entry>();
            _keyToIndexMap = new SerializedDictionary<K, int>(capacity > 0 ? 1 : 0);
            _indexToKeyMap = new SerializedDictionary<int, K>(capacity > 0 ? 1 : 0);
            _freeIndices = new List<int>();
        }

#endregion

#region VALUE

        public V GetValue(int index) {
            if (index < 0 || index >= _head) return default;

            ref var entry = ref _entries[index];
            return entry.IsDisposed() ? default : entry.value;
        }

        public bool TryGetValue(int index, out V value) {
            if (index < 0 || index >= _head) {
                value = default;
                return false;
            }

            ref var entry = ref _entries[index];

            if (entry.IsDisposed()) {
                value = default;
                return false;
            }

            value = entry.value;
            return true;
        }

        public ref V GetValueByRef(int index) {
            if (index < 0 || index >= _head) {
                throw new IndexOutOfRangeException($"Index {index} is out of range. " +
                                                   $"Must be non-negative and less than the size of the collection.");
            }

            ref var entry = ref _entries[index];

            if (entry.IsDisposed()) {
                throw new IndexOutOfRangeException($"Index {index} is out of range. " +
                                                   $"Must be non-negative and less than the size of the collection.");
            }

            return ref entry.value;
        }

        public void SetValue(int index, V value) {
            if (index < 0 || index >= _head) return;

            ref var entry = ref _entries[index];

            if (entry.IsDisposed()) return;

            entry.value = value;
        }

        public bool TrySetValue(int index, V value) {
            if (index < 0 || index >= _head) return false;

            ref var entry = ref _entries[index];

            if (entry.IsDisposed()) return false;

            entry.value = value;
            return true;
        }

#endregion

#region ROOT

        public Node GetOrAddRoot(K key) {
            bool hasKey = _keyToIndexMap.TryGetValue(key, out int index);

            if (!hasKey) {
                index = AllocateNode();

                _keyToIndexMap[key] = index;
                _indexToKeyMap[index] = key;
            }

            ref var entry = ref _entries[index];
            return new Node(index, entry.next, entry.child);
        }

        public Node GetRoot(K key) {
            if (!_keyToIndexMap.TryGetValue(key, out int index)) return InvalidNode;

            ref var entry = ref _entries[index];
            return new Node(index, entry.next, entry.child);
        }

        public bool TryGetRoot(K key, out Node node) {
            if (!_keyToIndexMap.TryGetValue(key, out int index)) {
                node = InvalidNode;
                return false;
            }

            ref var entry = ref _entries[index];
            node = new Node(index, entry.next, entry.child);
            return true;
        }

        public void RemoveRoot(K key) {
            if (!_keyToIndexMap.TryGetValue(key, out int index)) return;

            DisposeNodePath(index);
            ApplyDefragmentationIfNecessary();
        }

        public bool ContainsRoot(K key) {
            return _keyToIndexMap.ContainsKey(key);
        }

#endregion

#region NODE

        public Node AppendNext(int parent) {
            TryAppendNext(parent, out var node);
            return node;
        }

        public Node AppendChild(int parent) {
            TryAppendChild(parent, out var node);
            return node;
        }

        public bool TryAppendNext(int parent, out Node node) {
            if (parent < 0 || parent >= _head) {
                node = InvalidNode;
                return false;
            }

            ref var entry = ref _entries[parent];

            if (entry.IsDisposed()) {
                node = InvalidNode;
                return false;
            }

            while (true) {
                if (entry.next < 0) break;

                parent = entry.next;
                entry = ref _entries[parent];
            }

            int index = AllocateNode();
            entry.next = index;

            entry = ref _entries[index];
            entry.root = parent;

            node = new Node(index, entry.next, entry.child);
            return true;
        }

        public bool TryAppendChild(int parent, out Node node) {
            if (parent < 0 || parent >= _head) {
                node = InvalidNode;
                return false;
            }

            ref var entry = ref _entries[parent];

            if (entry.IsDisposed()) {
                node = InvalidNode;
                return false;
            }

            if (entry.child >= 0) return TryAppendNext(entry.child, out node);

            int index = AllocateNode();
            entry.child = index;

            entry = ref _entries[index];
            entry.root = parent;

            node = new Node(index, entry.next, entry.child);
            return true;
        }

        public Node GetNode(int index) {
            if (index < 0 || index >= _head) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError($"{nameof(LinearTreeMap<K, V>)}: " +
                               $"trying to get node by incorrect index {index}.");
#endif
                return InvalidNode;
            }

            ref var entry = ref _entries[index];

            if (entry.IsDisposed()) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError($"{nameof(LinearTreeMap<K, V>)}: " +
                               $"trying to get node by incorrect index {index}.");
#endif
                return InvalidNode;
            }

            return new Node(index, entry.next, entry.child);
        }

        public bool TryGetNode(int index, out Node node) {
            if (index < 0 || index >= _head) {
                node = InvalidNode;
                return false;
            }

            ref var entry = ref _entries[index];

            if (entry.IsDisposed()) {
                node = InvalidNode;
                return false;
            }

            node = new Node(index, entry.next, entry.child);
            return true;
        }

        public void RemoveNode(int index) {
            if (index < 0 || index >= _head) return;

            ref var entry = ref _entries[index];

            if (entry.IsDisposed()) return;

            DisposeNodePath(entry.child);

            int root = entry.root;
            int next = entry.next;

            if (next < 0) {
                if (root >= 0) {
                    ref var rootEntry = ref _entries[root];

                    if (rootEntry.child == index) rootEntry.child = -1;
                    else rootEntry.next = -1;
                }

                DisposeNode(index);
                ApplyDefragmentationIfNecessary();
                return;
            }

            ref var nextEntry = ref _entries[next];

            entry = nextEntry;
            entry.root = root;

            DisposeNode(next);
            ApplyDefragmentationIfNecessary();
        }

        public bool ContainsNode(int index) {
            if (index < 0 || index >= _head) return false;

            ref var entry = ref _entries[index];
            return !entry.IsDisposed();
        }

#endregion

#region STORAGE

        private int AllocateNode() {
            int index = -1;

            for (int i = _freeIndices.Count - 1; i >= 0; i++) {
                index = _freeIndices[i];
                _freeIndices.RemoveAt(i);

                if (index < _head) break;
            }

            if (index < 0 || index >= _head) {
                index = _head++;
                ArrayExtensions.EnsureCapacity(ref _entries, _head);
            }

            ref var entry = ref _entries[index];
            entry.Reset();

            _aliveCount++;

            return index;
        }

        private void DisposeNode(int index) {
            _aliveCount--;
            if (_aliveCount < 0) _aliveCount = 0;

            if (index == _head - 1) _head--;
            else _freeIndices.Add(index);

            ref var entry = ref _entries[index];
            entry.Dispose();

            if (_indexToKeyMap.TryGetValue(index, out var key)) {
                _indexToKeyMap.Remove(index);
                _keyToIndexMap.Remove(key);
            }
        }

        private void DisposeNodePath(int root) {
            var entry = new Entry();
            int head = _head;

            DisposeNodePathRecursively(ref entry, ref root, ref head);
        }

        private void DisposeNodePathRecursively(ref Entry root, ref int index, ref int head) {
            while (true) {
                if (index < 0 || index >= head) return;

                DisposeNode(index);

                root = ref _entries[index];
                index = root.child;
                int next = root.next;

                DisposeNodePathRecursively(ref root, ref index, ref head);

                index = next;
            }
        }

        private void ApplyDefragmentationIfNecessary() {
            if (_aliveCount <= _entries.Length * 0.5f) ApplyDefragmentation();
        }

        private void ApplyDefragmentation() {
            int lastAlive = _head - 1;
            int fi = _freeIndices.Count - 1;

            for (int i = lastAlive; i >= _aliveCount; i--) {
                ref var entry = ref _entries[i];
                if (entry.IsDisposed()) continue;

                int freeIndex = -1;
                for (; fi >= 0; fi--) {
                    freeIndex = _freeIndices[fi];
                    if (freeIndex < _aliveCount) break;
                }

                if (freeIndex < 0 || freeIndex >= _aliveCount) break;

                ref var freeEntry = ref _entries[freeIndex];
                freeEntry = entry;

                if (_indexToKeyMap.TryGetValue(i, out var key)) {
                    _indexToKeyMap.Remove(i);

                    _indexToKeyMap[freeIndex] = key;
                    _keyToIndexMap[key] = freeIndex;
                }

                if (entry.root >= 0) {
                    ref var rootEntry = ref _entries[entry.root];

                    if (rootEntry.child == i) rootEntry.child = freeIndex;
                    else rootEntry.next = freeIndex;
                }
            }

            _head = _aliveCount;
            _freeIndices.Clear();
            Array.Resize(ref _entries, _aliveCount);
        }

#endregion

    }

}
