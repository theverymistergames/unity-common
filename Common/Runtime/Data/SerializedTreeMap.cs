using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using MisterGames.Common.Lists;
using UnityEngine;

namespace MisterGames.Common.Data {

    [Serializable]
    public sealed class SerializedTreeMap<K, V> where K : struct, IEquatable<K> {

        #region DATA

        [SerializeField] private Node[] _nodes;
        [SerializeField] private SerializedDictionary<K, int> _rootIndexMap;
        [SerializeField] private SerializedDictionary<KeyIndex, int> _nodeIndexMap;
        [SerializeField] private List<int> _freeIndices;
        [SerializeField] private int _head;
        [SerializeField] private int _version;

        private static readonly Iterator InvalidIterator = new Iterator(null, -1, 0, 0);

        public Dictionary<K, int>.KeyCollection RootKeys => _rootIndexMap.Keys;
        public Dictionary<KeyIndex, int>.KeyCollection NodeKeys => _nodeIndexMap.Keys;

        public int Count => RootCount + NodeCount;
        public int RootCount => _rootIndexMap.Count;
        public int NodeCount => _nodeIndexMap.Count;

        #endregion

        #region DATA STRUCTURES

        [Serializable]
        public struct KeyIndex : IEquatable<KeyIndex> {

            public K key;
            public int index;

            public KeyIndex(K key, int index) {
                this.key = key;
                this.index = index;
            }

            public bool Equals(KeyIndex other) {
                return key.Equals(other.key) && index == other.index;
            }

            public override bool Equals(object obj) {
                return obj is KeyIndex other && Equals(other);
            }

            public override int GetHashCode() {
                return HashCode.Combine(key, index);
            }

            public override string ToString() {
                return $"{nameof(KeyIndex)}(key {key}, index {index})";
            }
        }

        [Serializable]
        public struct Node {

            [SerializeField] private K _key;

            public K key => _key;
            public V value;

            [SerializeField] internal int parent;
            [SerializeField] internal int child;
            [SerializeField] internal int previous;
            [SerializeField] internal int next;

            internal void Reset(K key) {
                _key = key;
                value = default;
                parent = -1;
                child = -1;
                previous = -1;
                next = -1;
            }

            internal void Dispose() {
                parent = -2;
            }

            internal bool IsDisposed() {
                return parent < -1;
            }

            public override string ToString() {
                return $"{nameof(Node)}(key {_key}, value {value}, parent {parent}, child {child}, previous {previous}, next {next})";
            }
        }

        public struct Iterator : IDisposable {

            public int Level => _level;

            private SerializedTreeMap<K, V> _map;
            private int _index;
            private int _level;
            private int _version;

            public Iterator(SerializedTreeMap<K, V> map, int index, int level, int version) {
                _map = map;
                _index = index;
                _level = level;
                _version = version;
            }

            public ref Node GetNode() {
                ThrowIfDisposed();

                return ref _map.GetNode(_index);
            }

            public bool MovePreOrder() {
                ThrowIfDisposed();

                if (!_map.TryGetPreOrder(_index, out int levelOffset, out int next)) return false;

                _index = next;
                _level += levelOffset;
                return true;
            }

            public bool MoveParent() {
                ThrowIfDisposed();

                if (!_map.TryGetParent(_index, out int parent)) return false;

                _index = parent;
                _level--;

                return true;
            }

            public bool MoveChild() {
                ThrowIfDisposed();

                if (!_map.TryGetChild(_index, out int child)) return false;

                _index = child;
                _level++;

                return true;
            }

            public bool MoveChild(K key) {
                ThrowIfDisposed();

                if (!_map.TryGetChild(_index, key, out int child)) return false;

                _index = child;
                _level++;

                return true;
            }

            public bool MoveNext() {
                ThrowIfDisposed();

                if (!_map.TryGetNext(_index, out int next)) return false;

                _index = next;
                return true;
            }

            public bool MovePrevious() {
                ThrowIfDisposed();

                if (!_map.TryGetPrevious(_index, out int previous)) return false;

                _index = previous;
                return true;
            }

            public void GetOrAddChild(K key) {
                ThrowIfDisposed();

                _map.GetOrAddChild(_index, key);
            }

            public void RemoveChild(K key) {
                ThrowIfDisposed();

                _map.RemoveChild(ref _index, key);
                _version = _map._version;
            }

            public bool ContainsChild(K key) {
                ThrowIfDisposed();

                return _map.ContainsChild(_index, key);
            }

            public int GetChildCount() {
                ThrowIfDisposed();

                return _map.GetChildCount(_index);
            }

            public void ClearChildren() {
                ThrowIfDisposed();

                _map.ClearChildren(ref _index);
                _version = _map._version;
            }

            public void Dispose() {
                _index = -1;
                _map = null;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void ThrowIfDisposed() {
                if (_index < 0 || _map == null || _map._version != _version) {
                    throw new InvalidOperationException("Iterator was already disposed and can not be used.");
                }
            }

            public override string ToString() {
                if (_index < 0 || _map == null || _map._version != _version) return $"Tree(version {_version}, disposed)";

                var it = this;
                var sb = new StringBuilder();

                sb.AppendLine($"Tree(version {_version}):");

                while (true) {
                    ref var node = ref GetNode();
                    sb.AppendLine($"{it._level}{new string('-', it._level * 2)} [{it._index}] {node}");

                    if (!it.MovePreOrder()) break;
                }

                return sb.ToString();
            }
        }

        #endregion

        #region CONSTRUCTORS

        public SerializedTreeMap(int roots = 0, int nodes = 0) {
            int totalCount = roots + nodes;

            _nodes = totalCount > 0 ? new Node[totalCount] : Array.Empty<Node>();
            _rootIndexMap = new SerializedDictionary<K, int>(roots);
            _nodeIndexMap = new SerializedDictionary<KeyIndex, int>(nodes);
            _freeIndices = new List<int>();
        }

        #endregion

        #region TREE

        public Iterator GetOrAddTree(K key) {
            if (!_rootIndexMap.TryGetValue(key, out int index)) {
                index = AllocateNode(key);
                _rootIndexMap[key] = index;
            }

            return new Iterator(this, index, 0, _version);
        }

        public Iterator GetTree(K key) {
            return _rootIndexMap.TryGetValue(key, out int index)
                ? new Iterator(this, index, 0, _version)
                : InvalidIterator;
        }

        public bool TryGetTree(K key, out Iterator iterator) {
            if (!_rootIndexMap.TryGetValue(key, out int index)) {
                iterator = InvalidIterator;
                return false;
            }

            iterator = new Iterator(this, index, 0, _version);
            return true;
        }

        #endregion

        #region ROOT

        public int GetOrAddRoot(K key) {
            if (!_rootIndexMap.TryGetValue(key, out int index)) {
                index = AllocateNode(key);
                _rootIndexMap[key] = index;
            }

            return index;
        }

        public int GetRoot(K key) {
            return _rootIndexMap.TryGetValue(key, out int index) ? index : -1;
        }

        public bool TryGetRoot(K key, out int index) {
            if (_rootIndexMap.TryGetValue(key, out index)) return true;

            index = -1;
            return false;
        }

        public void RemoveRoot(K key) {
            if (!_rootIndexMap.TryGetValue(key, out int index)) return;

            DisposeNodePath(index);
            ApplyDefragmentationIfNecessary();
        }

        public bool ContainsRoot(K key) {
            return _rootIndexMap.ContainsKey(key);
        }

        public void ClearAll() {
            _nodes = Array.Empty<Node>();
            _rootIndexMap.Clear();
            _nodeIndexMap.Clear();
            _freeIndices.Clear();
            _head = 0;
            _version++;
        }

        public void OptimizeLayout() {
            ApplyDefragmentation();
        }

        #endregion

        #region NODE

        public ref Node GetNode(int index) {
            return ref _nodes[index];
        }

        public int GetPreOrder(int index) {
            if (index < 0 || index >= _head) return -1;

            ref var node = ref _nodes[index];

            if (node.child >= 0) return node.child;
            if (node.next >= 0) return node.next;

            if (node.parent < 0) return -1;

            node = ref _nodes[node.parent];
            if (node.next < 0) return -1;

            return node.next;
        }

        public int GetParent(int child) {
            if (child < 0 || child >= _head) return -1;

            ref var node = ref _nodes[child];
            return node.parent;
        }

        public int GetChild(int parent) {
            if (parent < 0 || parent >= _head) return -1;

            ref var node = ref _nodes[parent];
            return node.child;
        }

        public int GetChild(int parent, K key) {
            if (_nodeIndexMap.TryGetValue(new KeyIndex(key, parent), out int child)) return child;
            return -1;
        }

        public int GetPrevious(int next) {
            if (next < 0 || next >= _head) return -1;

            ref var node = ref _nodes[next];
            return node.previous;
        }

        public int GetNext(int previous) {
            if (previous < 0 || previous >= _head) return -1;

            ref var node = ref _nodes[previous];
            return node.next;
        }

        public bool TryGetPreOrder(int index, out int levelOffset, out int next) {
            if (index < 0 || index >= _head) {
                next = -1;
                levelOffset = 0;
                return false;
            }

            ref var node = ref _nodes[index];

            if (node.child >= 0) {
                next = node.child;
                levelOffset = 1;
                return true;
            }

            if (node.next >= 0) {
                next = node.next;
                levelOffset = 0;
                return true;
            }

            if (node.parent < 0) {
                next = -1;
                levelOffset = 0;
                return false;
            }

            node = ref _nodes[node.parent];

            if (node.next < 0) {
                next = -1;
                levelOffset = 0;
                return false;
            }

            next = node.next;
            levelOffset = -1;
            return true;
        }

        public bool TryGetPreOrder(int index, out int next) {
            if (index < 0 || index >= _head) {
                next = -1;
                return false;
            }

            ref var node = ref _nodes[index];

            if (node.child >= 0) {
                next = node.child;
                return true;
            }

            if (node.next >= 0) {
                next = node.next;
                return true;
            }

            if (node.parent < 0) {
                next = -1;
                return false;
            }

            node = ref _nodes[node.parent];

            if (node.next < 0) {
                next = -1;
                return false;
            }

            next = node.next;
            return true;
        }

        public bool TryGetParent(int child, out int parent) {
            if (child < 0 || child >= _head) {
                parent = -1;
                return false;
            }

            ref var node = ref _nodes[child];
            parent = node.parent;

            return parent >= 0;
        }

        public bool TryGetChild(int parent, out int child) {
            if (parent < 0 || parent >= _head) {
                child = -1;
                return false;
            }

            ref var node = ref _nodes[parent];
            child = node.child;

            return child >= 0;
        }

        public bool TryGetChild(int parent, K key, out int child) {
            if (_nodeIndexMap.TryGetValue(new KeyIndex(key, parent), out child)) return true;

            child = -1;
            return false;
        }

        public bool TryGetPrevious(int next, out int previous) {
            if (next < 0 || next >= _head) {
                previous = -1;
                return false;
            }

            ref var node = ref _nodes[next];
            previous = node.previous;

            return previous >= 0;
        }

        public bool TryGetNext(int previous, out int next) {
            if (previous < 0 || previous >= _head) {
                next = -1;
                return false;
            }

            ref var node = ref _nodes[previous];
            next = node.next;

            return next >= 0;
        }

        public int GetOrAddChild(int parent, K key) {
            if (parent < 0 || parent >= _head) return -1;

            ref var node = ref _nodes[parent];
            if (node.IsDisposed()) return -1;

            var nodeKey = new KeyIndex(key, parent);
            if (_nodeIndexMap.TryGetValue(nodeKey, out int index)) return index;

            index = AllocateNode(key);
            _nodeIndexMap[nodeKey] = index;

            node = ref _nodes[parent];
            int child = node.child;

            if (child >= 0) {
                while (true) {
                    node = ref _nodes[child];
                    if (node.next < 0) break;

                    child = node.next;
                }

                node.next = index;
            }
            else {
                node.child = index;
            }

            node = ref _nodes[index];
            node.parent = parent;
            node.previous = child;

            return index;
        }

        public void RemoveChild(ref int parent, K key) {
            if (parent < 0 || parent >= _head) return;

            ref var node = ref _nodes[parent];
            if (node.IsDisposed()) return;

            if (!_nodeIndexMap.TryGetValue(new KeyIndex(key, parent), out int index)) return;

            node = ref _nodes[index];
            int next = node.next;
            int previous = node.previous;

            DisposeNodePath(index);

            if (previous >= 0) {
                node = ref _nodes[previous];
                node.next = next;

                if (next >= 0) {
                    node = ref _nodes[next];
                    node.previous = previous;
                }
            }
            else {
                node = ref _nodes[parent];
                node.child = next;
            }

            parent = ApplyDefragmentationIfNecessary(parent);

            _version++;
        }

        public int GetChildCount(int parent) {
            if (parent < 0 || parent >= _head) return 0;

            ref var node = ref _nodes[parent];
            if (node.IsDisposed()) return 0;

            parent = node.child;
            int count = 0;

            while (parent >= 0) {
                count++;
                node = ref _nodes[parent];
                parent = node.next;
            }

            return count;
        }

        public bool ContainsChild(int parent, K key) {
            return _nodeIndexMap.ContainsKey(new KeyIndex(key, parent));
        }

        public void ClearChildren(ref int parent) {
            if (parent < 0 || parent >= _head) return;

            DisposeNodePath(parent, false);
            parent = ApplyDefragmentationIfNecessary(parent);

            _version++;
        }

        #endregion

        #region STORAGE

        private int AllocateNode(K key) {
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
            node.Reset(key);

            return index;
        }

        private void DisposeNode(ref Node node, int index) {
            if (index == _head - 1) _head--;
            else _freeIndices.Add(index);

            if (node.parent < 0) _rootIndexMap.Remove(node.key);
            else _nodeIndexMap.Remove(new KeyIndex(node.key, node.parent));

            node.Dispose();
        }

        private void DisposeNodePath(int root, bool disposeRoot = true) {
            if (root < 0 || root >= _head) return;

            ref var node = ref _nodes[root];
            int child = node.child;

            if (disposeRoot) DisposeNode(ref node, root);
            if (child < 0) return;

            int pointer = child;

            while (true) {
                node = ref _nodes[pointer];

                child = node.child;
                int next = node.next;
                int parent = node.parent;

                DisposeNode(ref node, pointer);

                if (child >= 0) {
                    pointer = child;
                    continue;
                }

                if (next >= 0) {
                    pointer = next;
                    continue;
                }

                if (parent < 0) return;
                if (parent == root) return;

                node = ref _nodes[parent];
                next = node.next;

                if (next < 0) return;

                pointer = next;
            }
        }

        private int ApplyDefragmentationIfNecessary(int index = -1) {
            return Count <= _nodes.Length * 0.5f ? ApplyDefragmentation(index) : index;
        }

        private int ApplyDefragmentation(int index = -1) {
            int j = _freeIndices.Count - 1;
            int count = Count;

            for (int i = _head - 1; i >= count; i--) {
                ref var node = ref _nodes[i];
                if (node.IsDisposed()) continue;

                int freeIndex = -1;
                for (; j >= 0; j--) {
                    freeIndex = _freeIndices[j];
                    if (freeIndex < count) break;
                }

                if (freeIndex < 0 || freeIndex >= count) break;

                ref var freeNode = ref _nodes[freeIndex];
                freeNode = node;

                if (index == i) index = freeIndex;

                int parent = node.parent;
                int child = node.child;
                int previous = node.previous;
                int next = node.next;

                if (parent >= 0) {
                    node = ref _nodes[parent];
                    if (node.child == i) node.child = freeIndex;

                    _nodeIndexMap[new KeyIndex(node.key, node.parent)] = freeIndex;
                }
                else {
                    _rootIndexMap[node.key] = freeIndex;
                }

                if (child >= 0) {
                    node = ref _nodes[child];
                    node.parent = freeIndex;
                }

                if (previous >= 0) {
                    node = ref _nodes[previous];
                    node.next = freeIndex;
                }

                if (next >= 0) {
                    node = ref _nodes[next];
                    node.previous = freeIndex;
                }
            }

            _head = count;
            _freeIndices.Clear();
            Array.Resize(ref _nodes, _head);

            _version++;

            return index;
        }

        #endregion

        public override string ToString() {
            var sb = new StringBuilder();
            sb.AppendLine($"{nameof(SerializedTreeMap<K, V>)}(version {_version}, roots {RootCount}, nodes {NodeCount}):");

            for (int i = 0; i < _head; i++) {
                ref var node = ref _nodes[i];
                if (node.IsDisposed()) continue;

                sb.AppendLine($"[{i}] :: {node}");
            }

            return sb.ToString();
        }
    }

}
