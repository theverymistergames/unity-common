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

            public int Index => _index;
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

            public bool MoveIndex(int index) {
                ThrowIfDisposed();

                if (!_map.ContainsNode(index)) return false;

                _index = index;
                _level = _map.GetDepth(index);
                return true;
            }

            public bool MovePreOrder(int root = -1) {
                ThrowIfDisposed();

                if (_map.TryGetChild(_index, out int child)) {
                    _index = child;
                    _level++;
                    return true;
                }

                if (_index == root) return false;

                if (_map.TryGetNext(_index, out int next)) {
                    _index = next;
                    return true;
                }

                child = _index;
                int level = _level;

                while (_map.TryGetParent(child, out int parent)) {
                    if (parent == root) return false;

                    level--;

                    if (_map.TryGetNext(parent, out int nextParent)) {
                        _index = nextParent;
                        _level = level;
                        return true;
                    }

                    child = parent;
                }

                return false;
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

            public int GetParent() {
                ThrowIfDisposed();

                return _map.GetParent(_index);
            }

            public int GetChild() {
                ThrowIfDisposed();

                return _map.GetChild(_index);
            }

            public int GetChild(K key) {
                ThrowIfDisposed();

                return _map.GetChild(_index, key);
            }

            public int GetPrevious() {
                ThrowIfDisposed();

                return _map.GetPrevious(_index);
            }

            public int GetNext() {
                ThrowIfDisposed();

                return _map.GetNext(_index);
            }

            public bool TryGetParent(out int parent) {
                ThrowIfDisposed();

                return _map.TryGetParent(_index, out parent);
            }

            public bool TryGetChild(out int child) {
                ThrowIfDisposed();

                return _map.TryGetChild(_index, out child);
            }

            public bool TryGetChild(K key, out int child) {
                ThrowIfDisposed();

                return _map.TryGetChild(_index, key, out child);
            }

            public bool TryGetPrevious(out int previous) {
                ThrowIfDisposed();

                return _map.TryGetPrevious(_index, out previous);
            }

            public bool TryGetNext(out int next) {
                ThrowIfDisposed();

                return _map.TryGetNext(_index, out next);
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
            public bool IsInvalid() {
                return _index < 0 || _map == null || _map._version != _version;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void ThrowIfDisposed() {
                if (IsInvalid()) throw new InvalidOperationException("Iterator is invalid");
            }

            public override string ToString() {
                if (IsInvalid()) return $"Tree(version {_version}, invalid, index {_index})";

                int root = _index;
                var it = this;
                var sb = new StringBuilder();

                sb.AppendLine($"Tree(version {_version}):");

                while (true) {
                    ref var node = ref it.GetNode();
                    sb.AppendLine($"{it.Level}{new string('-', it.Level * 2)} [{it._index}] {node}");

                    if (!it.MovePreOrder(root)) break;
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

        public Iterator GetTree(K key) {
            return _rootIndexMap.TryGetValue(key, out int index)
                ? new Iterator(this, index, 0, _version)
                : InvalidIterator;
        }

        public bool TryGetTree(K key, out Iterator iterator) {
            iterator = GetTree(key);
            return !iterator.IsInvalid();
        }

        public Iterator GetTree(int parent, K key) {
            return _nodeIndexMap.TryGetValue(new KeyIndex(key, parent), out int index)
                ? new Iterator(this, index, GetDepth(index), _version)
                : InvalidIterator;
        }

        public bool TryGetTree(int parent, K key, out Iterator iterator) {
            iterator = GetTree(parent, key);
            return !iterator.IsInvalid();
        }

        #endregion

        #region ROOT

        public int GetOrAddRoot(K key) {
            if (_rootIndexMap.TryGetValue(key, out int index)) return index;

            index = AllocateNode(key);
            _rootIndexMap[key] = index;

            return index;
        }

        public int GetRoot(K key) {
            return _rootIndexMap.TryGetValue(key, out int index) ? index : -1;
        }

        public bool TryGetRoot(K key, out int index) {
            index = GetRoot(key);
            return index >= 0;
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

        #region CHILD

        public int GetChild(int parent) {
            if (parent < 0 || parent >= _head) return -1;

            ref var node = ref _nodes[parent];
            if (node.IsDisposed()) return -1;

            return node.child;
        }

        public int GetChild(int parent, K key) {
            return _nodeIndexMap.TryGetValue(new KeyIndex(key, parent), out int child) ? child : -1;
        }

        public bool TryGetChild(int parent, out int child) {
            child = GetChild(parent);
            return child >= 0;
        }

        public bool TryGetChild(int parent, K key, out int child) {
            child = GetChild(parent, key);
            return child >= 0;
        }

        public int GetOrAddChild(int parent, K key) {
            var nodeKey = new KeyIndex(key, parent);
            if (_nodeIndexMap.TryGetValue(nodeKey, out int index)) return index;

            if (parent < 0 || parent >= _head) return -1;

            ref var node = ref _nodes[parent];
            if (node.IsDisposed()) return -1;

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

        public void RemoveChild(int parent, K key) {
            if (_nodeIndexMap.TryGetValue(new KeyIndex(key, parent), out int index)) RemoveNode(index, out parent);
        }

        public void RemoveChild(ref int parent, K key) {
            if (_nodeIndexMap.TryGetValue(new KeyIndex(key, parent), out int index)) RemoveNode(index, out parent);
        }

        public void ClearChildren(int parent) {
            ClearChildren(ref parent);
        }

        public void ClearChildren(ref int parent) {
            if (parent < 0 || parent >= _head) return;

            ref var node = ref _nodes[parent];
            if (node.IsDisposed()) return;

            DisposeNodePath(parent, false);
            parent = ApplyDefragmentationIfNecessary(parent);

            node = ref _nodes[parent];
            node.child = -1;

            _version++;
        }

        #endregion

        #region NODE

        public ref Node GetNode(int index) {
            return ref _nodes[index];
        }
        
        public int GetDepth(int index) {
            if (index < 0 || index >= _head) return 0;

            ref var node = ref _nodes[index];
            if (node.IsDisposed()) return 0;

            int depth = 0;

            while (true) {
                if (node.parent < 0) break;

                depth++;
                node = ref _nodes[node.parent];
            }

            return depth;
        }

        public int GetParent(int child) {
            if (child < 0 || child >= _head) return -1;

            ref var node = ref _nodes[child];
            if (node.IsDisposed()) return -1;

            return node.parent;
        }

        public int GetPrevious(int next) {
            if (next < 0 || next >= _head) return -1;

            ref var node = ref _nodes[next];
            if (node.IsDisposed()) return -1;

            return node.previous;
        }

        public int GetNext(int previous) {
            if (previous < 0 || previous >= _head) return -1;

            ref var node = ref _nodes[previous];
            if (node.IsDisposed()) return -1;

            return node.next;
        }

        public bool TryGetParent(int child, out int parent) {
            parent = GetParent(child);
            return parent >= 0;
        }

        public bool TryGetPrevious(int next, out int previous) {
            previous = GetPrevious(next);
            return previous >= 0;
        }

        public bool TryGetNext(int previous, out int next) {
            next = GetNext(previous);
            return next >= 0;
        }

        public void RemoveNode(int index) {
            RemoveNode(index, out _);
        }

        public void RemoveNode(int index, out int parent) {
            if (index < 0 || index >= _head) {
                parent = -1;
                return;
            }

            ref var node = ref _nodes[index];
            if (node.IsDisposed()) {
                parent = -1;
                return;
            }

            int next = node.next;
            int previous = node.previous;
            parent = node.parent;

            DisposeNodePath(index);

            if (next >= 0) {
                node = ref _nodes[next];
                node.previous = previous;
            }

            if (previous >= 0) {
                node = ref _nodes[previous];
                node.next = next;
            }
            else if (parent >= 0) {
                node = ref _nodes[parent];
                node.child = next;
            }

            parent = ApplyDefragmentationIfNecessary(parent);
            _version++;
        }

        public bool ContainsNode(int index) {
            if (index < 0 || index >= _head) return false;

            ref var node = ref _nodes[index];
            return !node.IsDisposed();
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

                while (true) {
                    if (parent < 0 || parent == root) return;

                    node = ref _nodes[parent];
                    next = node.next;

                    if (next < 0) {
                        parent = node.parent;
                        continue;
                    }

                    pointer = next;
                    break;
                }
            }
        }

        private int ApplyDefragmentationIfNecessary(int index = -1) {
            return Count <= _nodes.Length * 0.5f ? ApplyDefragmentation(index) : index;
        }

        private int ApplyDefragmentation(int index = -1) {
            //return index;

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
                    _nodeIndexMap[new KeyIndex(node.key, parent)] = freeIndex;

                    node = ref _nodes[parent];
                    if (node.child == i) node.child = freeIndex;
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
            sb.AppendLine($"{nameof(SerializedTreeMap<K, V>)}(version {_version}, roots {RootCount}, nodes {NodeCount})");

            foreach (var root in RootKeys) {
                sb.AppendLine(GetTree(root).ToString());
            }

            return sb.ToString();
        }
    }

}
