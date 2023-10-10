using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using MisterGames.Common.Lists;
using UnityEngine;

namespace MisterGames.Common.Data {

    /// <summary>
    /// Multi-level dictionary.
    /// Top-level entries are called roots and trees, they can be retrieved by key.
    /// All the other entries are called children, they can be retrieved by key and parent index.
    ///
    /// Data is stored as graph, each node can have one child node link and one next node link.
    /// First child of a node is connected by child link, next children are connected by next link to the previous child.
    /// The all children hold link to the parent.
    ///
    /// Tree iterator can be used to go through tree and perform get, add, remove operations.
    /// Iterator can be marked as invalid, if it detects changes with the map, that happened outside.
    /// Invalid iterator throws <see cref="InvalidOperationException"/> on any operation.
    ///
    /// Get and add by key operations on any level have O(1) complexity.
    /// Remove childless node operation also has O(1) complexity. If node has children,
    /// there is a need to clear the whole subtree with the root in the removed node, so operation takes
    /// about N iterations, where N is the total amount of nodes in a subtree.
    ///
    /// When after node removal alive nodes count is less than a half of total nodes count,
    /// defragmentation operation is applied to cleanup unused nodes.
    ///
    /// </summary>
    /// <typeparam name="K">Type of keys</typeparam>
    /// <typeparam name="V">Type of values</typeparam>
    [Serializable]
    public sealed class TreeMap<K, V> {

        #region DATA

        [SerializeField] private Node[] _nodes;
        [SerializeField] private SerializedDictionary<K, int> _rootIndexMap;
        [SerializeField] private SerializedDictionary<KeyIndex, int> _nodeIndexMap;
        [SerializeField] private int _head;
        [SerializeField] private int _version;

        private readonly List<int> _freeIndices;

        private static readonly Iterator InvalidIterator = new Iterator(null, -1, 0, 0);

        /// <summary>
        /// Total count of nodes.
        /// </summary>
        public int Count => _rootIndexMap.Count + _nodeIndexMap.Count;

        /// <summary>
        /// Keys of the root nodes.
        /// </summary>
        public IReadOnlyCollection<K> Keys => _rootIndexMap.Keys;

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
                return EqualityComparer<K>.Default.Equals(key, other.key) && index == other.index;
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

        /// <summary>
        /// Struct that holds node key and value.
        /// </summary>
        [Serializable]
        public struct Node {

            [SerializeField] private K _key;

            /// <summary>
            /// Read-only node key.
            /// </summary>
            public K key => _key;

            /// <summary>
            /// Node value.
            /// </summary>
            public V value;

            [SerializeField] internal int parent;
            [SerializeField] internal int child;
            [SerializeField] internal int next;
            [SerializeField] internal int prev;

            internal void Reset(K key) {
                _key = key;
                value = default;
                parent = -1;
                child = -1;
                next = -1;
                prev = -1;
            }

            internal void Dispose() {
                prev = -2;
            }

            internal bool IsDisposed() {
                return prev < -1;
            }

            public override string ToString() {
                return IsDisposed()
                    ? $"{nameof(Node)}(disposed)"
                    : $"{nameof(Node)}(key {_key}, value {value}, parent {parent}, child {child}, next {next}, prev {prev})";
            }
        }

        /// <summary>
        /// Use iterator to walk through tree and perform get, add and remove nodes,
        /// get key, get and set value operations. Iterator caches current node index and level.
        /// Iterator can become invalid, if some operations were performed with the map outside of iterator.
        /// </summary>
        public struct Iterator : IDisposable {

            /// <summary>
            /// Index of the current node.
            /// </summary>
            public int Index => _index;

            /// <summary>
            /// Level of the current node. Root level is 0, its children level is 1, etc.
            /// </summary>
            public int Level => _level;

            private TreeMap<K, V> _map;
            private int _index;
            private int _level;
            private int _version;

            public Iterator(TreeMap<K, V> map, int index, int level, int version) {
                _map = map;
                _index = index;
                _level = level;
                _version = version;
            }

            /// <summary>
            /// Get current node by ref.
            /// </summary>
            /// <returns>Current node reference</returns>
            public ref Node GetNode() {
                ThrowIfDisposed();

                return ref _map.GetNode(_index);
            }

            /// <summary>
            /// Get current node key.
            /// </summary>
            /// <returns>Current node key</returns>
            public K GetKey() {
                ThrowIfDisposed();

                return _map.GetKey(_index);
            }

            /// <summary>
            /// Get current node value.
            /// </summary>
            /// <returns>Current node value</returns>
            public V GetValue() {
                ThrowIfDisposed();

                return _map.GetValue(_index);
            }

            /// <summary>
            /// Set value of current node.
            /// </summary>
            /// <param name="value">New value</param>
            public void SetValue(V value) {
                ThrowIfDisposed();

                _map.SetValue(_index, value);
            }

            /// <summary>
            /// Move iterator to a new index, if it is valid in map.
            /// </summary>
            /// <param name="index">Index of the target node in map</param>
            /// <returns>True if moved to the node with passed index</returns>
            public bool MoveIndex(int index) {
                ThrowIfDisposed();

                if (!_map.ContainsIndex(index)) return false;

                _index = index;
                _level = _map.GetDepth(index);
                return true;
            }

            /// <summary>
            /// Move iterator to the next node in pre-order:
            /// if current node has child node, move to child;
            /// if current node has next node, move to next;
            /// otherwise move to next parent node.
            /// </summary>
            /// <param name="root">When root is reached from its last child, pre-order stops</param>
            /// <returns>True if moved in pre-order</returns>
            public bool MovePreOrder(int root = -1) {
                ThrowIfDisposed();

                ref var node = ref _map.GetNode(_index);

                if (node.child >= 0) {
                    _index = node.child;
                    _level++;
                    return true;
                }

                if (_index == root) return false;

                if (node.next >= 0) {
                    _index = node.next;
                    return true;
                }

                int parent = node.parent;
                int level = _level;

                while (true) {
                    if (parent < 0 || parent == root) return false;

                    node = ref _map.GetNode(parent);
                    level--;

                    if (node.next >= 0) {
                        parent = node.next;
                        break;
                    }

                    parent = node.parent;
                }

                _index = parent;
                _level = level;

                return true;
            }

            /// <summary>
            /// Move iterator to the node parent. Root nodes have no parent.
            /// </summary>
            /// <returns>True if moved to the parent node</returns>
            public bool MoveParent() {
                ThrowIfDisposed();

                if (!_map.TryGetParentIndex(_index, out int parent)) return false;

                _index = parent;
                _level--;
                return true;
            }

            /// <summary>
            /// Move iterator to the first child node.
            /// </summary>
            /// <returns>True if moved to the first child node</returns>
            public bool MoveChild() {
                ThrowIfDisposed();

                if (!_map.TryGetChildIndex(_index, out int child)) return false;

                _index = child;
                _level++;
                return true;
            }

            /// <summary>
            /// Move iterator to the child node with passed key.
            /// </summary>
            /// <param name="key">Key of the child</param>
            /// <returns>True if moved to the child node with passed key</returns>
            public bool MoveChild(K key) {
                ThrowIfDisposed();

                if (!_map.TryGetIndex(key, _index, out int child)) return false;

                _index = child;
                _level++;
                return true;
            }

            /// <summary>
            /// Move iterator to the child node with passed key if exists, otherwise add and move.
            /// </summary>
            /// <param name="key">Key of the child</param>
            public void MoveOrAddChild(K key) {
                ThrowIfDisposed();

                _index = _map.GetOrAddNode(key, _index);
                _level++;
            }

            /// <summary>
            /// Move iterator to the next node. Use to iterate through some node children.
            /// </summary>
            /// <returns>True if moved to the next node</returns>
            public bool MoveNext() {
                ThrowIfDisposed();

                if (!_map.TryGetNextIndex(_index, out int next)) return false;

                _index = next;
                return true;
            }

            /// <summary>
            /// Move iterator to the previous node. Use to iterate through some node children.
            /// </summary>
            /// <returns>True if moved to the previous node</returns>
            public bool MovePrevious() {
                ThrowIfDisposed();

                if (!_map.TryGetPreviousIndex(_index, out int previous)) return false;

                _index = previous;
                return true;
            }

            /// <summary>
            /// Get index of the current node parent.
            /// </summary>
            /// <returns>Parent node index if present, otherwise -1</returns>
            public int GetParentIndex() {
                ThrowIfDisposed();

                return _map.GetParentIndex(_index);
            }

            /// <summary>
            /// Get index of the current node first child.
            /// </summary>
            /// <returns>First child node index if present, otherwise -1</returns>
            public int GetChildIndex() {
                ThrowIfDisposed();

                return _map.GetChildIndex(_index);
            }

            /// <summary>
            /// Get index of the current node child with passed key.
            /// </summary>
            /// <param name="key">Key of the child</param>
            /// <returns>Child node index if present, otherwise -1</returns>
            public int GetChildIndex(K key) {
                ThrowIfDisposed();

                return _map.GetIndex(key, _index);
            }

            /// <summary>
            /// Get index of the previous node.
            /// </summary>
            /// <returns>Previous node index if present, otherwise -1</returns>
            public int GetPreviousIndex() {
                ThrowIfDisposed();

                return _map.GetPreviousIndex(_index);
            }

            /// <summary>
            /// Get index of the next node.
            /// </summary>
            /// <returns>Next node index if present, otherwise -1</returns>
            public int GetNextIndex() {
                ThrowIfDisposed();

                return _map.GetNextIndex(_index);
            }

            /// <summary>
            /// Get index of the current node parent.
            /// </summary>
            /// <param name="parent">Parent node index if present, otherwise -1</param>
            /// <returns>True if has parent node</returns>
            public bool TryGetParentIndex(out int parent) {
                ThrowIfDisposed();

                return _map.TryGetParentIndex(_index, out parent);
            }

            /// <summary>
            /// Get index of the current node first child.
            /// </summary>
            /// <param name="child">First child node index if present, otherwise -1</param>
            /// <returns>True if has children</returns>
            public bool TryGetChildIndex(out int child) {
                ThrowIfDisposed();

                return _map.TryGetChildIndex(_index, out child);
            }

            /// <summary>
            /// Get index of the current node child with passed key.
            /// </summary>
            /// <param name="key">Key of the child</param>
            /// <param name="child">Child node index if present, otherwise -1</param>
            /// <returns>True if has child node with passed key</returns>
            public bool TryGetChildIndex(K key, out int child) {
                ThrowIfDisposed();

                return _map.TryGetIndex(key, _index, out child);
            }

            /// <summary>
            /// Get index of the previous node.
            /// </summary>
            /// <param name="previous">Previous node index if present, otherwise -1</param>
            /// <returns>True if has previous node</returns>
            public bool TryGetPreviousIndex(out int previous) {
                ThrowIfDisposed();

                return _map.TryGetPreviousIndex(_index, out previous);
            }

            /// <summary>
            /// Get index of the next node.
            /// </summary>
            /// <param name="next">Next node index if present, otherwise -1</param>
            /// <returns>True if has next node</returns>
            public bool TryGetNextIndex(out int next) {
                ThrowIfDisposed();

                return _map.TryGetNextIndex(_index, out next);
            }

            /// <summary>
            /// Add child with passed key if it is not present, otherwise get.
            /// </summary>
            /// <param name="key">Key of the child</param>
            /// <returns>Added or present child node index</returns>
            public int GetOrAddChild(K key) {
                ThrowIfDisposed();

                return _map.GetOrAddNode(key, _index);
            }

            /// <summary>
            /// Remove child node by key. This operation can cause map version change.
            /// </summary>
            /// <param name="key">Key of the child</param>
            public bool RemoveChild(K key) {
                ThrowIfDisposed();

                bool removed = _map.RemoveNode(key, ref _index);
                _version = _map._version;

                return removed;
            }

            /// <summary>
            /// Check if current node contains child with passed key.
            /// </summary>
            /// <param name="key">Key of the child</param>
            /// <returns>True if contains child with passed key</returns>
            public bool ContainsChild(K key) {
                ThrowIfDisposed();

                return _map.ContainsKey(key, _index);
            }

            /// <summary>
            /// Iterate through current node children and get the amount.
            /// </summary>
            /// <returns>Amount of children of the current node</returns>
            public int GetChildCount() {
                ThrowIfDisposed();

                return _map.GetChildCount(_index);
            }

            /// <summary>
            /// Remove all children from current node. This operation can cause map version change.
            /// </summary>
            public void ClearChildren() {
                ThrowIfDisposed();

                _map.ClearChildren(ref _index);
                _version = _map._version;
            }

            /// <summary>
            /// Force dispose iterator to make it invalid.
            /// </summary>
            public void Dispose() {
                _index = -1;
                _map = null;
            }

            /// <summary>
            /// Iterator can be invalid, when something changed in the map outside of the iterator,
            /// or when it is constructed as invalid as the result of getting tree by non-existent key.
            /// </summary>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsInvalid() {
                return _index < 0 || _map == null || _map._version != _version;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void ThrowIfDisposed() {
                if (IsInvalid()) throw new InvalidOperationException("Iterator is invalid");
            }

            public override string ToString() {
                if (IsInvalid()) return $"Tree(invalid)";

                int root = _index;
                var it = this;
                var sb = new StringBuilder();

                sb.AppendLine($"Tree:");

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

        /// <summary>
        /// Initialize storages with passed capacity of roots and nodes.
        /// </summary>
        /// <param name="roots">Roots capacity</param>
        /// <param name="nodes">Nodes capacity</param>
        public TreeMap(int roots = 0, int nodes = 0) {
            int capacity = roots + nodes;
            _nodes = capacity > 0 ? new Node[capacity] : Array.Empty<Node>();

            _rootIndexMap = new SerializedDictionary<K, int>(roots);
            _nodeIndexMap = new SerializedDictionary<KeyIndex, int>(nodes);

            _freeIndices = new List<int>();
        }

        #endregion

        #region TREE

        /// <summary>
        /// Get iterator for root with passed key.
        /// </summary>
        /// <param name="key">Key of the root</param>
        /// <param name="tree">Iterator for root, or invalid iterator if passed key is not present</param>
        /// <returns>True if passed root key is valid</returns>
        public bool TryGetTree(K key, out Iterator tree) {
            if (!_rootIndexMap.TryGetValue(key, out int index)) {
                tree = InvalidIterator;
                return false;
            }

            tree = new Iterator(this, index, 0, _version);
            return true;
        }

        /// <summary>
        /// Get iterator for child with passed key and parent index.
        /// </summary>
        /// <param name="key">Key of the child</param>
        /// <param name="parent">Index of the parent node</param>
        /// <param name="tree">Iterator for child, or invalid iterator if passed key or parent index are not present</param>
        /// <returns>True if passed child key and parent index are valid</returns>
        public bool TryGetTree(K key, int parent, out Iterator tree) {
            if (!_nodeIndexMap.TryGetValue(new KeyIndex(key, parent), out int index)) {
                tree = InvalidIterator;
                return false;
            }

            tree = new Iterator(this, index, GetDepth(index), _version);
            return true;
        }

        /// <summary>
        /// Get iterator for node with passed key and parent index.
        /// Throws <see cref="KeyNotFoundException"/> if has no entry with such key and parent index.
        /// </summary>
        /// <param name="key">Key of the node</param>
        /// <param name="parent">Index of the parent node</param>
        /// <returns>Iterator for node</returns>
        public Iterator GetTree(K key, int parent = -1) {
            int index;

            if (parent < 0) {
                if (!_rootIndexMap.TryGetValue(key, out index)) {
                    throw new KeyNotFoundException($"{nameof(TreeMap<K, V>)}: root key {key} is not found");
                }
            }
            else if (!_nodeIndexMap.TryGetValue(new KeyIndex(key, parent), out index)) {
                throw new KeyNotFoundException($"{nameof(TreeMap<K, V>)}: child key {key} for parent at index {parent} is not found");
            }

            return new Iterator(this, index, GetDepth(index), _version);
        }

        /// <summary>
        /// Remove all trees. This operation can cause version change.
        /// </summary>
        public void Clear() {
            _nodes = Array.Empty<Node>();
            _rootIndexMap.Clear();
            _nodeIndexMap.Clear();
            _freeIndices.Clear();
            _head = 0;
            _version++;
        }

        #endregion

        #region NODE

        /// <summary>
        /// Get node key at index.
        /// Incorrect index can cause <see cref="IndexOutOfRangeException"/> or disposed node get.
        /// See if node is present by <see cref="ContainsIndex"/>.
        /// </summary>
        /// <param name="index">Index of the node</param>
        /// <returns>Node key</returns>
        public K GetKey(int index) {
            ref var node = ref _nodes[index];
            return node.key;
        }

        /// <summary>
        /// Get node value at index.
        /// Incorrect index can cause <see cref="IndexOutOfRangeException"/> or disposed node get.
        /// See if node is present by <see cref="ContainsIndex"/>.
        /// </summary>
        /// <param name="index">Index of the node</param>
        /// <returns>Node value</returns>
        public V GetValue(int index) {
            ref var node = ref _nodes[index];
            return node.value;
        }

        /// <summary>
        /// Set node value at index.
        /// Incorrect index can cause <see cref="IndexOutOfRangeException"/> or disposed node get.
        /// See if node is present by <see cref="ContainsIndex"/>.
        /// </summary>
        /// <param name="index">Index of the node</param>
        /// <param name="value">New value</param>
        public void SetValue(int index, V value) {
            ref var node = ref _nodes[index];
            node.value = value;
        }

        /// <summary>
        /// Get node by ref with passed index. Use node to get key, get and set value.
        /// Incorrect index can cause <see cref="IndexOutOfRangeException"/> or disposed node get.
        /// See if node is present by <see cref="ContainsIndex"/>.
        /// </summary>
        /// <param name="index">Index of the node</param>
        /// <returns>Reference to the node struct</returns>
        public ref Node GetNode(int index) {
            return ref _nodes[index];
        }

        /// <summary>
        /// Get depth of node with passed index. Root depth is 0, root child depth is 1, etc.
        /// This operation iterates through a tree up to the tree root to calculate depth.
        /// </summary>
        /// <param name="index">Node index</param>
        /// <returns>Depth of the node with passed index</returns>
        public int GetDepth(int index) {
            if (index < 0 || index >= _head) return 0;

            ref var node = ref _nodes[index];
            if (node.IsDisposed()) return 0;

            int depth = 0;

            while (node.parent >= 0) {
                node = ref _nodes[node.parent];
                depth++;
            }

            return depth;
        }

        /// <summary>
        /// Add node with passed key to the parent if it is not present, otherwise get.
        /// Throws <see cref="KeyNotFoundException"/> if parent index is invalid.
        /// </summary>
        /// <param name="key">Key of the node</param>
        /// <param name="parent">Index of the parent node</param>
        /// <returns>Node index if added or get, otherwise -1</returns>
        public int GetOrAddNode(K key, int parent = -1) {
            if (parent < 0) {
                if (_rootIndexMap.TryGetValue(key, out int root)) return root;

                root = AllocateNode(key);
                _rootIndexMap[key] = root;

                return root;
            }

            var nodeKey = new KeyIndex(key, parent);
            if (_nodeIndexMap.TryGetValue(nodeKey, out int index)) return index;

            if (parent >= _head) {
                throw new KeyNotFoundException($"{nameof(TreeMap<K, V>)}: parent at index {parent} is not found");
            }

            ref var node = ref _nodes[parent];
            if (node.IsDisposed()) {
                throw new KeyNotFoundException($"{nameof(TreeMap<K, V>)}: parent at index {parent} is not found");
            }

            index = AllocateNode(key);
            _nodeIndexMap[nodeKey] = index;

            node = ref _nodes[parent];
            int child = node.child;
            node.child = index;
            int next = -1;

            if (child >= 0) {
                node = ref _nodes[child];
                node.prev = index;
                next = child;
            }

            node = ref _nodes[index];
            node.next = next;
            node.parent = parent;

            return index;
        }

        /// <summary>
        /// Remove node by key and parent. This operation can cause map version change.
        /// Note that parent index can change after remove during defragmentation.
        /// </summary>
        /// <param name="key">Key of the node</param>
        /// <param name="parent">Index of the parent node</param>
        public bool RemoveNode(K key, int parent = -1) {
            return RemoveNode(key, ref parent);
        }

        /// <summary>
        /// Remove node by key and parent. This operation can cause map version change.
        /// Note that parent index can change after remove during defragmentation.
        /// </summary>
        /// <param name="key">Key of the removed node</param>
        /// <param name="parent">Index of the parent node</param>
        public bool RemoveNode(K key, ref int parent) {
            if (parent < 0) {
                if (!_rootIndexMap.TryGetValue(key, out int root)) return false;

                DisposeNodePath(root, disposeIndex: true);
                ApplyDefragmentationIfNecessary();

                _version++;
                return true;
            }

            if (!_nodeIndexMap.TryGetValue(new KeyIndex(key, parent), out int index)) return false;

            ref var node = ref _nodes[index];
            int prev = node.prev;
            int next = node.next;

            DisposeNodePath(index, disposeIndex: true);

            if (prev >= 0) {
                node = ref _nodes[prev];
                node.next = next;
            }
            else {
                node = ref _nodes[parent];
                node.child = next;
            }

            if (next >= 0) {
                node = ref _nodes[next];
                node.prev = prev;
            }

            parent = ApplyDefragmentationIfNecessary(parent);

            _version++;
            return true;
        }

        /// <summary>
        /// Remove all children from the parent node. This operation can cause map version change.
        /// Note that parent index can change after remove during defragmentation.
        /// </summary>
        /// <param name="parent">Index of the parent node</param>
        public void ClearChildren(int parent) {
            ClearChildren(ref parent);
        }

        /// <summary>
        /// Remove all children from the parent node. This operation can cause map version change.
        /// Note that parent index can change after remove during defragmentation.
        /// </summary>
        /// <param name="parent">Index of the parent node</param>
        public void ClearChildren(ref int parent) {
            if (parent < 0 || parent >= _head) return;

            ref var node = ref _nodes[parent];
            if (node.IsDisposed()) return;

            DisposeNodePath(parent, disposeIndex: false);

            parent = ApplyDefragmentationIfNecessary(parent);

            _version++;
        }

        /// <summary>
        /// Iterate through parent node children and get the amount.
        /// </summary>
        /// <param name="parent">Index of the parent node</param>
        /// <returns>Amount of children of the parent node</returns>
        public int GetChildCount(int parent) {
            if (parent < 0 || parent >= _head) return 0;

            ref var node = ref _nodes[parent];
            if (node.IsDisposed()) return 0;

            int child = node.child;
            int count = 0;

            while (child >= 0) {
                count++;
                node = ref _nodes[child];
                child = node.next;
            }

            return count;
        }

        /// <summary>
        /// Check if map contains node with passed key and parent index.
        /// </summary>
        /// <param name="key">Key of the node</param>
        /// <param name="parent">Index of the parent node</param>
        /// <returns>True if contains node</returns>
        public bool ContainsKey(K key, int parent = -1) {
            return parent < 0 ? _rootIndexMap.ContainsKey(key) : _nodeIndexMap.ContainsKey(new KeyIndex(key, parent));
        }

        #endregion

        #region INDEX

        /// <summary>
        /// Get index of the node with passed key and parent index.
        /// </summary>
        /// <param name="key">Key of the node</param>
        /// <param name="parent">Index of the parent node</param>
        /// <returns>Node index if present, otherwise -1</returns>
        public int GetIndex(K key, int parent = -1) {
            int index;
            if (parent < 0) return _rootIndexMap.TryGetValue(key, out index) ? index : -1;
            return _nodeIndexMap.TryGetValue(new KeyIndex(key, parent), out index) ? index : -1;
        }

        /// <summary>
        /// Get index of the child node with passed key and parent index.
        /// </summary>
        /// <param name="key">Key of the child</param>
        /// <param name="parent">Index of the parent node</param>
        /// <param name="index">Child node index if present, otherwise -1</param>
        /// <returns>True if parent has child with passed key</returns>
        public bool TryGetIndex(K key, int parent, out int index) {
            index = GetIndex(key, parent);
            return index >= 0;
        }

        /// <summary>
        /// Get root node with passed key.
        /// </summary>
        /// <param name="key">Key of the root node</param>
        /// <param name="index">Root node index if passed key is valid, otherwise -1</param>
        /// <returns>True if passed key is valid</returns>
        public bool TryGetIndex(K key, out int index) {
            index = GetIndex(key);
            return index >= 0;
        }

        /// <summary>
        /// Get index of node parent.
        /// </summary>
        /// <param name="child">Index of the child node</param>
        /// <returns>Parent node index if present, otherwise -1</returns>
        public int GetParentIndex(int child) {
            if (child < 0 || child >= _head) return -1;

            ref var node = ref _nodes[child];
            if (node.IsDisposed()) return -1;

            return node.parent;
        }

        /// <summary>
        /// Get index of node first child.
        /// </summary>
        /// <param name="parent">Index of the parent node</param>
        /// <returns>First child node index if present, otherwise -1</returns>
        public int GetChildIndex(int parent) {
            if (parent < 0 || parent >= _head) return -1;

            ref var node = ref _nodes[parent];
            if (node.IsDisposed()) return -1;

            return node.child;
        }

        /// <summary>
        /// Get index of the next node.
        /// </summary>
        /// <param name="previous">Index of the previous node</param>
        /// <returns>Next node index if present, otherwise -1</returns>
        public int GetNextIndex(int previous) {
            if (previous < 0 || previous >= _head) return -1;

            ref var node = ref _nodes[previous];
            if (node.IsDisposed()) return -1;

            return node.next;
        }

        /// <summary>
        /// Get index of previous node.
        /// </summary>
        /// <param name="next">Index of the next node</param>
        /// <returns>Previous node index if present, otherwise -1</returns>
        public int GetPreviousIndex(int next) {
            if (next < 0 || next >= _head) return -1;

            ref var node = ref _nodes[next];
            if (node.IsDisposed()) return -1;

            return node.prev;
        }

        /// <summary>
        /// Get index of node parent.
        /// </summary>
        /// <param name="child">Index of the child node</param>
        /// <param name="parent">Parent node index if present, otherwise -1</param>
        /// <returns>True if has parent</returns>
        public bool TryGetParentIndex(int child, out int parent) {
            parent = GetParentIndex(child);
            return parent >= 0;
        }

        /// <summary>
        /// Get index of node first child.
        /// </summary>
        /// <param name="parent">Index of the parent node</param>
        /// <param name="child">First child node index if present, otherwise -1</param>
        /// <returns>True if has children</returns>
        public bool TryGetChildIndex(int parent, out int child) {
            child = GetChildIndex(parent);
            return child >= 0;
        }

        /// <summary>
        /// Get index of the next node.
        /// </summary>
        /// <param name="previous">Index of the previous node</param>
        /// <param name="next">Next node index if present, otherwise -1</param>
        /// <returns>True if has next node</returns>
        public bool TryGetNextIndex(int previous, out int next) {
            next = GetNextIndex(previous);
            return next >= 0;
        }

        /// <summary>
        /// Get index of previous node.
        /// </summary>
        /// <param name="next">Index of the next node</param>
        /// <param name="previous">Previous node index if present, otherwise -1</param>
        /// <returns>True if has previous node</returns>
        public bool TryGetPreviousIndex(int next, out int previous) {
            previous = GetPreviousIndex(next);
            return previous >= 0;
        }

        /// <summary>
        /// Check if node with passed index is present and not disposed.
        /// </summary>
        /// <param name="index">Index of the node</param>
        /// <returns>True if map has node with passed index and node is not disposed</returns>
        public bool ContainsIndex(int index) {
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

        private void DisposeNodePath(int index, bool disposeIndex) {
            if (index < 0 || index >= _head) return;

            ref var node = ref _nodes[index];
            int pointer = node.child;

            if (disposeIndex) DisposeNode(ref node, index);
            node.child = -1;

            while (pointer >= 0 && pointer != index) {
                node = ref _nodes[pointer];
                DisposeNode(ref node, pointer);

                if (node.child >= 0) {
                    pointer = node.child;
                    continue;
                }

                if (node.next > 0) {
                    pointer = node.next;
                    continue;
                }

                int parent = node.parent;
                while (true) {
                    if (parent < 0 || parent == index) return;

                    node = ref _nodes[parent];

                    if (node.next >= 0) {
                        pointer = node.next;
                        break;
                    }

                    parent = node.parent;
                }
            }
        }

        private int ApplyDefragmentationIfNecessary(int trackedIndex = -1) {
            if (Count >= _nodes.Length * 0.5f) return trackedIndex;

            int j = _freeIndices.Count - 1;
            int count = Count;

            for (int i = _head - 1; i >= count; i--) {
                ref var node = ref _nodes[i];
                if (node.IsDisposed()) continue;

                int freeIndex = -1;
                while (j >= 0) {
                    freeIndex = _freeIndices[j--];
                    if (freeIndex < count) break;
                }

                if (freeIndex < 0 || freeIndex >= count) break;

                ref var freeNode = ref _nodes[freeIndex];
                freeNode = node;

                if (trackedIndex == i) trackedIndex = freeIndex;

                var key = node.key;
                int parent = node.parent;
                int child = node.child;
                int next = node.next;
                int prev = node.prev;

                if (parent >= 0) {
                    node = ref _nodes[parent];
                    if (node.child == i) node.child = freeIndex;

                    _nodeIndexMap[new KeyIndex(key, parent)] = freeIndex;
                }
                else {
                    _rootIndexMap[key] = freeIndex;
                }

                if (prev >= 0) {
                    node = ref _nodes[prev];
                    node.next = freeIndex;
                }

                if (next >= 0) {
                    node = ref _nodes[next];
                    node.prev = freeIndex;
                }

                while (child >= 0) {
                    node = ref _nodes[child];
                    node.parent = freeIndex;

                    _nodeIndexMap.Remove(new KeyIndex(node.key, i));
                    _nodeIndexMap[new KeyIndex(node.key, freeIndex)] = child;

                    child = node.next;
                }
            }

            _head = count;
            _freeIndices.Clear();
            Array.Resize(ref _nodes, _head);

            _version++;

            return trackedIndex;
        }

        #endregion

        public override string ToString() {
            var sb = new StringBuilder();
            sb.AppendLine($"{nameof(TreeMap<K, V>)}(count {Count})");

            foreach (var root in Keys) {
                sb.AppendLine(GetTree(root).ToString());
            }

            return sb.ToString();
        }
    }

}
