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
        [SerializeField] private Map<K, int> _rootIndexMap;
        [SerializeField] private Map<KeyIndex, int> _nodeIndexMap;
        [SerializeField] private int _count;
        [SerializeField] private int _head;
        [SerializeField] private int _version;

        private readonly List<int> _freeIndices;
        private bool _isDefragmentationAllowed;

        private static readonly Iterator InvalidIterator = new Iterator(null, -1, 0, 0);

        /// <summary>
        /// Total count of nodes.
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Keys of the root nodes.
        /// </summary>
        public IReadOnlyCollection<K> Roots => _rootIndexMap.Keys;

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

        [Serializable]
        private struct Node {

            public K key;
            public V value;

            public int parent;
            public int child;
            public int next;
            public int prev;

            public void Reset(K key) {
                this.key = key;
                value = default;
                parent = -1;
                child = -1;
                next = -1;
                prev = -1;
            }

            public void DisallowChildren() {
                child = -2;
            }

            public bool AreChildrenAllowed() {
                return child > -2;
            }

            public void Dispose() {
                prev = -2;
            }

            public bool IsDisposed() {
                return prev < -1;
            }

            public override string ToString() {
                return IsDisposed()
                    ? $"{nameof(Node)}(disposed)"
                    : $"{nameof(Node)}(key {key}, parent {parent}, child {child}, next {next}, prev {prev})";
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
            /// Get and set child node value by key.
            /// </summary>
            /// <param name="key"></param>
            public V this[K key] {
                get => GetValue(key);
                set => SetValue(key, value);
            }

            /// <summary>
            /// Get current node key.
            /// </summary>
            /// <returns>Current node key</returns>
            public K GetKey() {
                ThrowIfDisposed();

                return _map.GetKeyAt(_index);
            }

            /// <summary>
            /// Get current node value.
            /// </summary>
            /// <returns>Current node value</returns>
            public ref V GetValue() {
                ThrowIfDisposed();

                return ref _map.GetValueAt(_index);
            }

            /// <summary>
            /// Set value of current node.
            /// </summary>
            /// <param name="value">New value</param>
            public void SetValue(V value) {
                ThrowIfDisposed();

                _map.SetValueAt(_index, value);
            }

            /// <summary>
            /// Get child node value by key.
            /// </summary>
            /// <param name="key">Key of the child</param>
            /// <param name="value">Value of the child</param>
            /// <returns>True if has child node</returns>
            public bool TryGetValue(K key, out V value) {
                ThrowIfDisposed();

                return _map.TryGetValue(key, _index, out value);
            }

            /// <summary>
            /// Get child node value by key.
            /// </summary>
            /// <param name="key">Key of the child</param>
            public ref V GetValue(K key) {
                ThrowIfDisposed();

                return ref _map.GetValue(key, _index);
            }

            /// <summary>
            /// Set value of child node.
            /// </summary>
            /// <param name="key">Key of the child</param>
            /// <param name="value">New value</param>
            public void SetValue(K key, V value) {
                ThrowIfDisposed();

                _map.SetValue(key, _index, value);
            }

            /// <summary>
            /// Add a child without key with value.
            /// </summary>
            /// <param name="value">New child value</param>
            public void AddEndPoint(V value) {
                ThrowIfDisposed();

                _map.AddEndPoint(_index, value);
            }

            /// <summary>
            /// Move iterator to a new index, if it is valid in map.
            /// </summary>
            /// <param name="index">Index of the target node in map</param>
            /// <returns>True if moved to the node with passed index</returns>
            public bool MoveNode(int index) {
                ThrowIfDisposed();

                if (!_map.ContainsNodeAt(index)) return false;

                _index = index;
                _level = _map.GetNodeDepth(index);
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

                ref var node = ref _map._nodes[_index];

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

                    node = ref _map._nodes[parent];
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

                if (!_map.TryGetParent(_index, out int parent)) return false;

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

                if (!_map.TryGetChild(_index, out int child)) return false;

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

                if (!_map.TryGetNode(key, _index, out int child)) return false;

                _index = child;
                _level++;
                return true;
            }

            /// <summary>
            /// Move iterator to the next node. Use to iterate through some node children.
            /// </summary>
            /// <returns>True if moved to the next node</returns>
            public bool MoveNext() {
                ThrowIfDisposed();

                if (!_map.TryGetNext(_index, out int next)) return false;

                _index = next;
                return true;
            }

            /// <summary>
            /// Move iterator to the previous node. Use to iterate through some node children.
            /// </summary>
            /// <returns>True if moved to the previous node</returns>
            public bool MovePrevious() {
                ThrowIfDisposed();

                if (!_map.TryGetPrevious(_index, out int previous)) return false;

                _index = previous;
                return true;
            }

            /// <summary>
            /// Get index of the current node parent.
            /// </summary>
            /// <returns>Parent node index if present, otherwise -1</returns>
            public int GetParent() {
                ThrowIfDisposed();

                return _map.GetParent(_index);
            }

            /// <summary>
            /// Get index of the current node parent.
            /// </summary>
            /// <param name="parent">Parent node index if present, otherwise -1</param>
            /// <returns>True if has parent node</returns>
            public bool TryGetParent(out int parent) {
                ThrowIfDisposed();

                return _map.TryGetParent(_index, out parent);
            }

            /// <summary>
            /// Get index of the current node first child.
            /// </summary>
            /// <returns>First child node index if present, otherwise -1</returns>
            public int GetChild() {
                ThrowIfDisposed();

                return _map.GetChild(_index);
            }

            /// <summary>
            /// Get index of the current node child with passed key.
            /// </summary>
            /// <param name="key">Key of the child</param>
            /// <returns>Child node index if present, otherwise -1</returns>
            public int GetChild(K key) {
                ThrowIfDisposed();

                return _map.GetNode(key, _index);
            }

            /// <summary>
            /// Get index of the current node first child.
            /// </summary>
            /// <param name="child">First child node index if present, otherwise -1</param>
            /// <returns>True if has children</returns>
            public bool TryGetChild(out int child) {
                ThrowIfDisposed();

                return _map.TryGetChild(_index, out child);
            }

            /// <summary>
            /// Get index of the current node child with passed key.
            /// </summary>
            /// <param name="key">Key of the child</param>
            /// <param name="child">Child node index if present, otherwise -1</param>
            /// <returns>True if has child node with passed key</returns>
            public bool TryGetChild(K key, out int child) {
                ThrowIfDisposed();

                return _map.TryGetNode(key, _index, out child);
            }

            /// <summary>
            /// Get index of the next node.
            /// </summary>
            /// <returns>Next node index if present, otherwise -1</returns>
            public int GetNext() {
                ThrowIfDisposed();

                return _map.GetNext(_index);
            }

            /// <summary>
            /// Get index of the next node.
            /// </summary>
            /// <param name="next">Next node index if present, otherwise -1</param>
            /// <returns>True if has next node</returns>
            public bool TryGetNext(out int next) {
                ThrowIfDisposed();

                return _map.TryGetNext(_index, out next);
            }

            /// <summary>
            /// Get index of the previous node.
            /// </summary>
            /// <returns>Previous node index if present, otherwise -1</returns>
            public int GetPrevious() {
                ThrowIfDisposed();

                return _map.GetPrevious(_index);
            }

            /// <summary>
            /// Get index of the previous node.
            /// </summary>
            /// <param name="previous">Previous node index if present, otherwise -1</param>
            /// <returns>True if has previous node</returns>
            public bool TryGetPrevious(out int previous) {
                ThrowIfDisposed();

                return _map.TryGetPrevious(_index, out previous);
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

                return _map.ContainsNode(key, _index);
            }

            /// <summary>
            /// Check if current node contains 1 or more children.
            /// </summary>
            /// <returns>True if current node contains children</returns>
            public bool ContainsChildren() {
                ThrowIfDisposed();

                return _map.ContainsChildren(_index);
            }

            /// <summary>
            /// Iterate through current node children and get the amount.
            /// </summary>
            /// <returns>Amount of children of the current node</returns>
            public int GetChildCount() {
                ThrowIfDisposed();

                return _map.GetChildrenCount(_index);
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
                    sb.AppendLine($"{it.Level}{new string('-', it.Level * 2)} [{it._index}] key {it.GetKey()} value {it.GetValue()}");
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
            _rootIndexMap = new Map<K, int>(roots);
            _nodeIndexMap = new Map<KeyIndex, int>(nodes);
            _freeIndices = new List<int>();

            _isDefragmentationAllowed = true;
        }

        /// <summary>
        /// Initialize with empty storages.
        /// </summary>
        public TreeMap() {
            _nodes = Array.Empty<Node>();
            _rootIndexMap = new Map<K, int>();
            _nodeIndexMap = new Map<KeyIndex, int>();
            _freeIndices = new List<int>();

            _isDefragmentationAllowed = true;
        }

        /// <summary>
        /// Initialize from source.
        /// </summary>
        /// <param name="source">Source TreeMap</param>
        public TreeMap(TreeMap<K, V> source) {
            _nodes = new Node[source._nodes.Length];
            Array.Copy(source._nodes, _nodes, _nodes.Length);

            _rootIndexMap = new Map<K, int>(source._rootIndexMap);
            _nodeIndexMap = new Map<KeyIndex, int>(source._nodeIndexMap);
            _freeIndices = new List<int>(source._freeIndices);

            _count = source._count;
            _head = source._head;

            _isDefragmentationAllowed = true;
        }

        #endregion

        #region TREE

        /// <summary>
        /// Create a tree copy from index as root node.
        /// </summary>
        /// <param name="index">Index of the root</param>
        /// <param name="includeRoot">If true, tree copy will contain one root,
        /// otherwise children of root will become roots of new tree.</param>
        /// <returns>TreeMap instance</returns>
        public TreeMap<K, V> Copy(int index, bool includeRoot = true) {
            if (index < 0 || index >= _head) return null;

            ref var node = ref _nodes[index];
            if (node.IsDisposed()) return null;

            var treeMap = new TreeMap<K, V>();

            int root = index;
            int parent = -1;
            index = node.child;

            if (includeRoot) {
                parent = treeMap.GetOrAddNode(node.key);
                treeMap.SetValueAt(parent, node.value);
            }

            while (index >= 0) {
                node = ref _nodes[index];

                int i = treeMap.GetOrAddNode(node.key, parent);
                treeMap.SetValueAt(i, node.value);

                if (node.child >= 0) {
                    parent = i;
                    index = node.child;
                    continue;
                }

                if (index == root) break;

                if (node.next >= 0) {
                    index = node.next;
                    continue;
                }

                index = node.parent;
                treeMap.TryGetParent(i, out i);

                while (index >= 0 && index != root) {
                    node = ref _nodes[index];

                    if (node.next >= 0) {
                        index = node.next;
                        treeMap.TryGetParent(i, out parent);
                        break;
                    }

                    index = node.parent;
                    treeMap.TryGetParent(i, out i);
                }

                if (index == root) break;
            }

            return treeMap;
        }

        /// <summary>
        /// Get or add and get iterator for node with passed key and parent index.
        /// </summary>
        /// <param name="key">Key of the root</param>
        /// <param name="parent">Index of the parent node</param>
        /// <returns>Iterator for existent or just added tree</returns>
        public Iterator GetOrAddTree(K key, int parent = -1) {
            int index;

            if (parent < 0) {
                if (!_rootIndexMap.TryGetValue(key, out index)) index = GetOrAddNode(key);
            }
            else {
                if (!_nodeIndexMap.TryGetValue(new KeyIndex(key, parent), out index)) index = GetOrAddNode(key, parent);
            }

            return new Iterator(this, index, GetNodeDepth(index), _version);
        }

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

            tree = new Iterator(this, index, GetNodeDepth(index), _version);
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

            return new Iterator(this, index, GetNodeDepth(index), _version);
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
            _count = 0;
            _version++;
        }

        #endregion

        #region KEY VALUE

        /// <summary>
        /// Get and set root node value by key.
        /// </summary>
        /// <param name="key"></param>
        public V this[K key] {
            get => GetValue(key);
            set => SetValue(key, value);
        }

        /// <summary>
        /// Get node key at index.
        /// </summary>
        /// <param name="index">Index of the node</param>
        /// <returns>Node key</returns>
        public K GetKeyAt(int index) {
            if (index < 0 || index >= _head) {
                throw new KeyNotFoundException($"{nameof(TreeMap<K, V>)}: value at index {index} is not found");
            }

            ref var node = ref _nodes[index];
            if (node.IsDisposed()) {
                throw new KeyNotFoundException($"{nameof(TreeMap<K, V>)}: value at index {index} is not found");
            }

            return node.key;
        }

        /// <summary>
        /// Get node value at index.
        /// </summary>
        /// <param name="index">Index of the node</param>
        /// <param name="value">Value of the node</param>
        /// <returns>True if has node</returns>
        public bool TryGetValueAt(int index, out V value) {
            if (index < 0 || index >= _head) {
                value = default;
                return false;
            }

            ref var node = ref _nodes[index];
            if (node.IsDisposed()) {
                value = default;
                return false;
            }

            value = node.value;
            return true;
        }

        /// <summary>
        /// Get node value at index.
        /// </summary>
        /// <param name="index">Index of the node</param>
        /// <returns>Node value</returns>
        public ref V GetValueAt(int index) {
            if (index < 0 || index >= _head) {
                throw new KeyNotFoundException($"{nameof(TreeMap<K, V>)}: value at index {index} is not found");
            }

            ref var node = ref _nodes[index];
            if (node.IsDisposed()) {
                throw new KeyNotFoundException($"{nameof(TreeMap<K, V>)}: value at index {index} is not found");
            }

            return ref node.value;
        }

        /// <summary>
        /// Set node value at index.
        /// </summary>
        /// <param name="index">Index of the node</param>
        /// <param name="value">New value</param>
        public void SetValueAt(int index, V value) {
            if (index < 0 || index >= _head) {
                throw new KeyNotFoundException($"{nameof(TreeMap<K, V>)}: node at index {index} is not found");
            }

            ref var node = ref _nodes[index];
            if (node.IsDisposed()) {
                throw new KeyNotFoundException($"{nameof(TreeMap<K, V>)}: node at index {index} is not found");
            }

            node.value = value;
        }

        /// <summary>
        /// Get root node value by key.
        /// </summary>
        /// <param name="key">Key of the node</param>
        /// <param name="value">Value of the node</param>
        /// <returns>True if has node</returns>
        public bool TryGetValue(K key, out V value) {
            if (_rootIndexMap.TryGetValue(key, out int index)) {
                ref var node = ref _nodes[index];
                value = node.value;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Get node value by key and parent index.
        /// </summary>
        /// <param name="key">Key of the node</param>
        /// <param name="parent">Index of the parent node</param>
        /// <param name="value">Value of the node</param>
        /// <returns>True if has node</returns>
        public bool TryGetValue(K key, int parent, out V value) {
            if (parent < 0) {
                if (_rootIndexMap.TryGetValue(key, out int index)) {
                    ref var node = ref _nodes[index];
                    value = node.value;
                    return true;
                }
            }
            else {
                if (_nodeIndexMap.TryGetValue(new KeyIndex(key, parent), out int index)) {
                    ref var node = ref _nodes[index];
                    value = node.value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Get node value by key and parent index.
        /// </summary>
        /// <param name="key">Key of the node</param>
        /// <param name="parent">Index of the parent node</param>
        /// <returns>Node value</returns>
        public ref V GetValue(K key, int parent = -1) {
            int index;

            if (parent < 0) {
                if (!_rootIndexMap.TryGetValue(key, out index)) {
                    throw new KeyNotFoundException($"{nameof(TreeMap<K, V>)}: root {key} is not found");
                }
            }
            else if (!_nodeIndexMap.TryGetValue(new KeyIndex(key, parent), out index)) {
                throw new KeyNotFoundException($"{nameof(TreeMap<K, V>)}: node {key} of parent at index {parent} is not found");
            }

            ref var node = ref _nodes[index];
            return ref node.value;
        }

        /// <summary>
        /// Set root value by key.
        /// </summary>
        /// <param name="key">Key of the root</param>
        /// <param name="value">New value</param>
        public void SetValue(K key, V value) {
            if (!_rootIndexMap.TryGetValue(key, out int index)) index = GetOrAddNode(key);

            ref var node = ref _nodes[index];
            node.value = value;
        }

        /// <summary>
        /// Set node value by key and parent index.
        /// </summary>
        /// <param name="key">Key of the root</param>
        /// <param name="parent">Index of the parent node</param>
        /// <param name="value">New value</param>
        public void SetValue(K key, int parent, V value) {
            if (!_nodeIndexMap.TryGetValue(new KeyIndex(key, parent), out int index)) index = GetOrAddNode(key, parent);

            ref var node = ref _nodes[index];
            node.value = value;
        }

        #endregion

        #region NODE

         /// <summary>
        /// Get index of the node with passed key and parent index.
        /// </summary>
        /// <param name="key">Key of the node</param>
        /// <param name="parent">Index of the parent node</param>
        /// <returns>Node index if present, otherwise -1</returns>
        public int GetNode(K key, int parent = -1) {
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
        public bool TryGetNode(K key, int parent, out int index) {
            index = GetNode(key, parent);
            return index >= 0;
        }

        /// <summary>
        /// Get root node with passed key.
        /// </summary>
        /// <param name="key">Key of the root node</param>
        /// <param name="index">Root node index if passed key is valid, otherwise -1</param>
        /// <returns>True if passed key is valid</returns>
        public bool TryGetNode(K key, out int index) {
            index = GetNode(key);
            return index >= 0;
        }

        /// <summary>
        /// Get node index, that is found first by predicate.
        /// </summary>
        /// <param name="target">Target object to compare with key and value of node</param>
        /// <param name="predicate">Func to describe comparison</param>
        /// <typeparam name="T">Type of the target object</typeparam>
        /// <returns>Node index or -1 if not found</returns>
        public int FindNode<T>(T target, Func<T, K, V, bool> predicate) {
            for (int i = 0; i < _head; i++) {
                ref var node = ref _nodes[i];
                if (node.IsDisposed()) continue;

                if (predicate.Invoke(target, node.key, node.value)) return i;
            }

            return -1;
        }

        /// <summary>
        /// Get index of node parent.
        /// </summary>
        /// <param name="child">Index of the child node</param>
        /// <returns>Parent node index if present, otherwise -1</returns>
        public int GetParent(int child) {
            if (child < 0 || child >= _head) return -1;

            ref var node = ref _nodes[child];
            if (node.IsDisposed()) return -1;

            return node.parent;
        }

        /// <summary>
        /// Get index of node parent.
        /// </summary>
        /// <param name="child">Index of the child node</param>
        /// <param name="parent">Parent node index if present, otherwise -1</param>
        /// <returns>True if has parent</returns>
        public bool TryGetParent(int child, out int parent) {
            parent = GetParent(child);
            return parent >= 0;
        }

        /// <summary>
        /// Get index of node first child.
        /// </summary>
        /// <param name="parent">Index of the parent node</param>
        /// <returns>First child node index if present, otherwise -1</returns>
        public int GetChild(int parent) {
            if (parent < 0 || parent >= _head) return -1;

            ref var node = ref _nodes[parent];
            if (node.IsDisposed()) return -1;

            return node.child;
        }

        /// <summary>
        /// Get index of node first child.
        /// </summary>
        /// <param name="parent">Index of the parent node</param>
        /// <param name="child">First child node index if present, otherwise -1</param>
        /// <returns>True if has children</returns>
        public bool TryGetChild(int parent, out int child) {
            child = GetChild(parent);
            return child >= 0;
        }

        /// <summary>
        /// Get index of the next node.
        /// </summary>
        /// <param name="previous">Index of the previous node</param>
        /// <returns>Next node index if present, otherwise -1</returns>
        public int GetNext(int previous) {
            if (previous < 0 || previous >= _head) return -1;

            ref var node = ref _nodes[previous];
            if (node.IsDisposed()) return -1;

            return node.next;
        }

        /// <summary>
        /// Get index of the next node.
        /// </summary>
        /// <param name="previous">Index of the previous node</param>
        /// <param name="next">Next node index if present, otherwise -1</param>
        /// <returns>True if has next node</returns>
        public bool TryGetNext(int previous, out int next) {
            next = GetNext(previous);
            return next >= 0;
        }

        /// <summary>
        /// Get index of previous node.
        /// </summary>
        /// <param name="next">Index of the next node</param>
        /// <returns>Previous node index if present, otherwise -1</returns>
        public int GetPrevious(int next) {
            if (next < 0 || next >= _head) return -1;

            ref var node = ref _nodes[next];
            if (node.IsDisposed()) return -1;

            return node.prev;
        }

        /// <summary>
        /// Get index of previous node.
        /// </summary>
        /// <param name="next">Index of the next node</param>
        /// <param name="previous">Previous node index if present, otherwise -1</param>
        /// <returns>True if has previous node</returns>
        public bool TryGetPrevious(int next, out int previous) {
            previous = GetPrevious(next);
            return previous >= 0;
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
        /// Insert node after previous by key and indices of parent and previous nodes.
        /// Throws <see cref="KeyNotFoundException"/> if parent or previous node index is invalid.
        /// </summary>
        /// <param name="key">Key of the child node</param>
        /// <param name="parent">Index of the parent node</param>
        /// <param name="previous">Index of the previous node, can be -1 to insert as first child</param>
        /// <returns>Node index if moved or inserted</returns>
        public int InsertNextNode(K key, int parent, int previous = -1) {
            if (parent < 0 || parent >= _head) {
                throw new KeyNotFoundException($"{nameof(TreeMap<K, V>)}: node at index {parent} is not found");
            }

            ref var node = ref _nodes[parent];
            if (node.IsDisposed()) {
                throw new KeyNotFoundException($"{nameof(TreeMap<K, V>)}: node at index {parent} is not found");
            }

            int child = node.child;
            int next = child;

            if (previous >= 0) {
                if (previous >= _head) {
                    throw new KeyNotFoundException($"{nameof(TreeMap<K, V>)}: node at index {previous} is not found");
                }

                node = ref _nodes[previous];

                if (node.IsDisposed() ||
                    !_nodeIndexMap.TryGetValue(new KeyIndex(node.key, parent), out int prev) ||
                    previous != prev
                ) {
                    throw new KeyNotFoundException($"{nameof(TreeMap<K, V>)}: node at index {previous} is not found");
                }

                next = node.next;
            }

            var nodeKey = new KeyIndex(key, parent);

            if (_nodeIndexMap.TryGetValue(nodeKey, out int index)) {
                node = ref _nodes[index];

                int lastNext = node.next;
                int lastPrev = node.prev;

                if (lastPrev >= 0) {
                    node = ref _nodes[lastPrev];
                    node.next = lastNext;
                }
                else {
                    node = ref _nodes[parent];
                    node.child = lastNext;
                }

                if (lastNext >= 0) {
                    node = ref _nodes[lastNext];
                    node.next = lastPrev;
                }
            }
            else {
                index = AllocateNode(key);
                _nodeIndexMap[nodeKey] = index;
            }

            node = ref _nodes[index];

            node.next = next;
            node.prev = previous;
            node.parent = parent;

            if (previous >= 0) {
                node = ref _nodes[previous];
                node.next = index;
            }
            else {
                node = ref _nodes[parent];
                node.child = index;
            }

            if (next >= 0) {
                node = ref _nodes[next];
                node.prev = index;
            }

            return index;
        }

        /// <summary>
        /// Add node without key to the parent.
        /// Throws <see cref="KeyNotFoundException"/> if parent index is invalid.
        /// </summary>
        /// <param name="parent">Index of the parent node</param>
        /// <param name="value">Value of the child node</param>
        /// <returns>Node index if added or get, otherwise -1</returns>
        public int AddEndPoint(int parent, V value) {
            if (parent < 0 || parent >= _head) {
                throw new KeyNotFoundException($"{nameof(TreeMap<K, V>)}: parent at index {parent} is not found");
            }

            ref var node = ref _nodes[parent];
            if (node.IsDisposed()) {
                throw new KeyNotFoundException($"{nameof(TreeMap<K, V>)}: parent at index {parent} is not found");
            }

            int index = AllocateNode();

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
            node.value = value;

            node.DisallowChildren();

            return index;
        }

        /// <summary>
        /// Remove node by key and parent. This operation can cause map version change.
        /// Note that parent index can change after remove during defragmentation.
        /// </summary>
        /// <param name="key">Key of the node</param>
        /// <param name="parent">Index of the parent node</param>
        /// <returns>True if node was removed</returns>
        public bool RemoveNode(K key, int parent = -1) {
            return RemoveNodeAt(GetNode(key, parent), out parent);
        }

        /// <summary>
        /// Remove node by key and parent. This operation can cause map version change.
        /// Note that parent index can change after remove during defragmentation.
        /// </summary>
        /// <param name="key">Key of the removed node</param>
        /// <param name="parent">Index of the parent node</param>
        /// <returns>True if node was removed</returns>
        public bool RemoveNode(K key, ref int parent) {
            return RemoveNodeAt(GetNode(key, parent), out parent);
        }

        /// <summary>
        /// Remove node at index. This operation can cause map version change.
        /// Note that parent index can change after remove during defragmentation.
        /// </summary>
        /// <param name="index">Index of the node</param>
        /// <returns>True if node was removed</returns>
        public bool RemoveNodeAt(int index) {
            return RemoveNodeAt(index, out _);
        }

        /// <summary>
        /// Remove node at index. This operation can cause map version change.
        /// Note that parent index can change after remove during defragmentation.
        /// </summary>
        /// <param name="index">Index of the node</param>
        /// <param name="parent">Index of the parent node</param>
        /// <returns>True if node was removed</returns>
        public bool RemoveNodeAt(int index, out int parent) {
            if (index < 0 || index >= _head) {
                parent = -1;
                return false;
            }

            ref var node = ref _nodes[index];

            if (node.IsDisposed()) {
                parent = -1;
                return false;
            }

            parent = node.parent;
            int prev = node.prev;
            int next = node.next;

            DisposeNodePath(index, disposeIndex: true);

            if (parent >= 0) {
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
            }

#if UNITY_EDITOR
            if (_isDefragmentationAllowed) parent = ApplyDefragmentation(parent);
#else
            if (_isDefragmentationAllowed) parent = ApplyDefragmentationIfNecessary(parent);
#endif

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

#if UNITY_EDITOR
            if (_isDefragmentationAllowed) parent = ApplyDefragmentation(parent);
#else
            if (_isDefragmentationAllowed) parent = ApplyDefragmentationIfNecessary(parent);
#endif

            _version++;
        }

        /// <summary>
        /// Iterate through parent node children and get the amount.
        /// </summary>
        /// <param name="parent">Index of the parent node</param>
        /// <returns>Amount of children of the parent node</returns>
        public int GetChildrenCount(int parent) {
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
        /// Check if current node contains 1 or more children.
        /// </summary>
        /// <returns>True if current node contains children</returns>
        public bool ContainsChildren(int parent) {
            if (parent < 0 || parent >= _head) return false;

            ref var node = ref _nodes[parent];
            if (node.IsDisposed()) return false;

            return node.child >= 0;
        }

        /// <summary>
        /// Check if node with passed key and parent index is present and not disposed.
        /// </summary>
        /// <param name="key">Key of the node</param>
        /// <param name="parent">Index of the parent node</param>
        /// <returns>True if map has node and node is not disposed</returns>
        public bool ContainsNode(K key, int parent = -1) {
            return ContainsNodeAt(GetNode(key, parent));
        }

        /// <summary>
        /// Check if node with passed index is present and not disposed.
        /// </summary>
        /// <param name="index">Index of the node</param>
        /// <returns>True if map has node with passed index and node is not disposed</returns>
        public bool ContainsNodeAt(int index) {
            if (index < 0 || index >= _head) return false;

            ref var node = ref _nodes[index];
            return !node.IsDisposed();
        }

        /// <summary>
        /// Get depth of node at index. Root depth is 0, root child depth is 1, etc.
        /// This operation iterates through a tree up to the tree root to calculate depth.
        /// </summary>
        /// <param name="index">Node index</param>
        /// <returns>Depth of the node with passed index</returns>
        public int GetNodeDepth(int index) {
            if (index < 0 || index >= _head) {
                throw new KeyNotFoundException($"{nameof(TreeMap<K, V>)}: node at index {index} is not found");
            }

            ref var node = ref _nodes[index];
            if (node.IsDisposed()) {
                throw new KeyNotFoundException($"{nameof(TreeMap<K, V>)}: node at index {index} is not found");
            }

            int depth = 0;

            while (node.parent >= 0) {
                node = ref _nodes[node.parent];
                depth++;
            }

            return depth;
        }

        /// <summary>
        /// Sort children by value. If comparer is null, default comparer is used.
        /// </summary>
        /// <param name="parent">Index of the parent node</param>
        /// <param name="comparer">Value comparer</param>
        /// <exception cref="KeyNotFoundException">If parent index is invalid</exception>
        public void SortChildren(int parent, IComparer<V> comparer = null) {
            if (parent < 0 || parent >= _head) {
                throw new KeyNotFoundException($"{nameof(TreeMap<K, V>)}: parent at index {parent} is not found");
            }

            ref var node = ref _nodes[parent];
            if (node.IsDisposed()) {
                throw new KeyNotFoundException($"{nameof(TreeMap<K, V>)}: parent at index {parent} is not found");
            }

            int child = node.child;
            if (child < 0) return;

            comparer ??= Comparer<V>.Default;
            node = ref _nodes[child];

            child = node.next;

            while (child >= 0) {
                node = ref _nodes[child];

                int next = node.next;
                int prev = node.prev;

                int c = child;
                int p = prev;

                while (p >= 0) {
                    ref var head = ref _nodes[p];
                    if (comparer.Compare(node.value, head.value) >= 0) break;

                    c = p;
                    p = head.prev;
                }

                if (p != prev) {
                    node.next = c;
                    node.prev = p;

                    if (p >= 0) {
                        node = ref _nodes[p];
                        node.next = child;
                    }
                    else if (node.parent >= 0) {
                        node = ref _nodes[node.parent];
                        node.child = child;
                    }

                    node = ref _nodes[c];
                    node.prev = child;

                    node = ref _nodes[prev];
                    node.next = next;

                    if (next >= 0) {
                        node = ref _nodes[next];
                        node.prev = prev;
                    }
                }

                child = next;
            }

            _version++;
        }

        #endregion

        #region STORAGE

        public void AllowDefragmentation(bool isAllowed) {
            _isDefragmentationAllowed = isAllowed;

#if UNITY_EDITOR
            if (isAllowed) ApplyDefragmentation();
#else
            if (isAllowed) ApplyDefragmentationIfNecessary();
#endif
        }

        private int AllocateNode(K key = default) {
            int index = -1;

            for (int i = _freeIndices.Count - 1; i >= 0; i--) {
                index = _freeIndices[i];
                _freeIndices.RemoveAt(i);

                if (index < _head - 1) break;
                if (index == _head - 1) _head--;
            }

            if (index < 0 || index >= _head) {
                index = _head++;

#if UNITY_EDITOR
                ArrayExtensions.EnsureCapacity(ref _nodes, _head, _head);
#else
                ArrayExtensions.EnsureCapacity(ref _nodes, _head);
#endif
            }

            ref var node = ref _nodes[index];

            node.Reset(key);
            _count++;

            return index;
        }

        private void DisposeNode(ref Node node, int index) {
            if (index == _head - 1) _head--;
            else _freeIndices.Add(index);

            if (node.parent < 0) _rootIndexMap.Remove(node.key);
            else if (node.AreChildrenAllowed()) _nodeIndexMap.Remove(new KeyIndex(node.key, node.parent));

            node.Dispose();
            _count--;
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
            return Count >= _nodes.Length * 0.5f ? trackedIndex : ApplyDefragmentation(trackedIndex);
        }

        private int ApplyDefragmentation(int trackedIndex = -1) {
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
                    if (node.AreChildrenAllowed()) _nodeIndexMap[new KeyIndex(key, parent)] = freeIndex;

                    node = ref _nodes[parent];
                    if (node.child == i) node.child = freeIndex;
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

                    if (node.AreChildrenAllowed()) {
                        _nodeIndexMap.Remove(new KeyIndex(node.key, i));
                        _nodeIndexMap[new KeyIndex(node.key, freeIndex)] = child;
                    }

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

            foreach (var root in Roots) {
                sb.AppendLine(GetTree(root).ToString());
            }

            return sb.ToString();
        }
    }

}
