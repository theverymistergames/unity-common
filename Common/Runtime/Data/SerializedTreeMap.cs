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

        private static readonly Iterator InvalidIterator = new Iterator(null, -1, 0);

        [SerializeField] private Node[] _nodes;
        [SerializeField] private SerializedDictionary<K, int> _rootIndexMap;
        [SerializeField] private SerializedDictionary<KeyIndex, int> _nodeIndexMap;
        [SerializeField] private List<int> _freeIndices;
        [SerializeField] private int _head;
        [SerializeField] private int _aliveCount;
        [SerializeField] private int _version;

        #endregion

        #region DATA STRUCTURES

        [Serializable]
        private struct KeyIndex : IEquatable<KeyIndex> {

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

            private SerializedTreeMap<K, V> _map;
            private int _version;
            private int _index;

            public Iterator(SerializedTreeMap<K, V> map, int index, int version) {
                _map = map;
                _version = version;
                _index = index;
            }

            public ref Node GetNode() {
                ThrowIfDisposed();

                return ref _map.GetNode(_index);
            }

            public bool MoveParent() {
                ThrowIfDisposed();

                if (!_map.TryGetParent(_index, out int parent)) return false;

                _index = parent;
                return true;
            }

            public bool MoveChild() {
                ThrowIfDisposed();

                if (!_map.TryGetChild(_index, out int child)) return false;

                _index = child;
                return true;
            }

            public bool MoveChild(K key) {
                ThrowIfDisposed();

                if (!_map.TryGetChild(_index, key, out int child)) return false;

                _index = child;
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

                ref var node = ref GetNode();

                var it = this;
                if (!it.MoveChild()) return $"Tree(version {_version}, root [{_index}] {node})";

                var sb = new StringBuilder();
                sb.AppendLine($"Tree(version {_version}, root [{_index}] {node}):");

                int level = 1;

                while (true) {
                    if (it.MoveChild()) level++;
                    else if (!it.MoveNext()) {
                        if (it.MoveParent()) level--;
                        if (!it.MoveNext()) break;
                    }

                    node = ref GetNode();
                    sb.AppendLine($"{level}{new string('-', level * 2)} [{it._index}] {node}");
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

            return new Iterator(this, index, _version);
        }

        public Iterator GetTree(K key) {
            _rootIndexMap.TryGetValue(key, out int index);
            return index >= 0 ? new Iterator(this, index, _version) : InvalidIterator;
        }

        public bool TryGetTree(K key, out Iterator iterator) {
            if (!_rootIndexMap.TryGetValue(key, out int index)) {
                iterator = InvalidIterator;
                return false;
            }

            iterator = new Iterator(this, index, _version);
            return true;
        }

        #endregion

        #region ROOT

        public int RootCount => _rootIndexMap.Count;

        public Dictionary<K, int>.KeyCollection RootKeys => _rootIndexMap.Keys;

        public int GetOrAddRoot(K key) {
            if (!_rootIndexMap.TryGetValue(key, out int index)) {
                index = AllocateNode(key);
                _rootIndexMap[key] = index;
            }

            return index;
        }

        public int GetRoot(K key) {
            _rootIndexMap.TryGetValue(key, out int index);
            return index;
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

        #endregion

        #region NODE

        public ref Node GetNode(int index) {
            return ref _nodes[index];
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
            return _nodeIndexMap.TryGetValue(new KeyIndex(key, parent), out child);
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

        #endregion

        #region STORAGE

        public void OptimizeLayout() {
            ApplyDefragmentation();
        }

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

            _aliveCount++;

            return index;
        }

        private void DisposeNodePath(int index) {
            var entry = new Node();
            DisposeNodePathRecursively(ref entry, ref index);
        }

        private void DisposeNodePathRecursively(ref Node node, ref int index) {
            while (true) {
                if (index < 0 || index >= _head) return;

                node = ref _nodes[index];

                _aliveCount--;
                if (_aliveCount < 0) _aliveCount = 0;

                if (index == _head - 1) _head--;
                else _freeIndices.Add(index);

                if (node.parent < 0) _rootIndexMap.Remove(node.key);
                else _nodeIndexMap.Remove(new KeyIndex(node.key, node.parent));

                node.Dispose();

                index = node.child;
                int next = node.next;

                DisposeNodePathRecursively(ref node, ref index);

                index = next;
            }
        }

        private int ApplyDefragmentationIfNecessary(int index = -1) {
            return _aliveCount <= _nodes.Length * 0.5f ? ApplyDefragmentation(index) : index;
        }

        private int ApplyDefragmentation(int index = -1) {
            int j = _freeIndices.Count - 1;

            for (int i = _head - 1; i >= _aliveCount; i--) {
                ref var node = ref _nodes[i];
                if (node.IsDisposed()) continue;

                int freeIndex = -1;
                for (; j >= 0; j--) {
                    freeIndex = _freeIndices[j];
                    if (freeIndex < _aliveCount) break;
                }

                if (freeIndex < 0 || freeIndex >= _aliveCount) break;

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

            _head = _aliveCount;
            _freeIndices.Clear();
            Array.Resize(ref _nodes, _head);

            _version++;

            return index;
        }

        #endregion

        public override string ToString() {
            var sb = new StringBuilder();
            sb.AppendLine($"{nameof(SerializedTreeMap<K, V>)}(version {_version}, roots {RootCount}, nodes {_aliveCount}):");

            foreach (var root in RootKeys) {
                sb.AppendLine(GetTree(root).ToString());
            }

            return sb.ToString();
        }
    }

}
