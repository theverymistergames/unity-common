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
    /// The first and the last child hold link to the parent.
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
    /// Defragmentation can be force-called via <see cref="SerializedTreeMap{K,V}.OptimizeLayout"/>.
    ///
    /// </summary>
    /// <typeparam name="K">Type of keys</typeparam>
    /// <typeparam name="V">Type of values</typeparam>
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

        /// <summary>
        /// Total count of nodes. This is a sum of root nodes count and child nodes count.
        /// </summary>
        public int Count => RootCount + NodeCount;

        /// <summary>
        /// Root nodes count.
        /// </summary>
        public int RootCount => _rootIndexMap.Count;

        /// <summary>
        /// Child nodes count.
        /// </summary>
        public int NodeCount => _nodeIndexMap.Count;

        /// <summary>
        /// Keys of the root nodes.
        /// </summary>
        public IReadOnlyCollection<K> RootKeys => _rootIndexMap.Keys;

        /// <summary>
        /// Keys of the child nodes. Child node key contains child key and parent index.
        /// </summary>
        public IReadOnlyCollection<KeyIndex> NodeKeys => _nodeIndexMap.Keys;

        #endregion

        #region DATA STRUCTURES

        /// <summary>
        /// Struct that is used to address child nodes by child key and parent index.
        /// </summary>
        [Serializable]
        public struct KeyIndex : IEquatable<KeyIndex> {

            /// <summary>
            /// Child node key.
            /// </summary>
            public K key;

            /// <summary>
            /// Parent node index.
            /// </summary>
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

            [SerializeField] internal int prev;
            [SerializeField] internal int child;
            [SerializeField] internal int last;
            [SerializeField] internal int next;

            internal void Reset(K key) {
                _key = key;
                value = default;
                prev = -1;
                child = -1;
                last = -1;
                next = -1;
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
                    : $"{nameof(Node)}(key {_key}, value {value}, prev {prev}, child {child}, last {last}, next {next})";
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

            /// <summary>
            /// Get current node by ref. Use node to get key, get and set value.
            /// </summary>
            /// <returns>Reference to the current node struct</returns>
            public ref Node GetNode() {
                ThrowIfDisposed();

                return ref _map.GetNode(_index);
            }

            /// <summary>
            /// Move iterator to a new index, if it is valid in map.
            /// </summary>
            /// <param name="index">Index of the target node in map</param>
            /// <returns>True if moved to the node with passed index</returns>
            public bool MoveIndex(int index) {
                ThrowIfDisposed();

                if (!_map.ContainsNode(index)) return false;

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

                int next = node.next;
                if (next < 0) return false;

                int index = _index;
                int level = _level;

                while (true) {
                    if (next < 0 || next == root) return false;

                    node = ref _map.GetNode(next);
                    if (node.last != index) break;

                    level--;

                    index = next;
                    next = node.next;
                }

                _index = next;
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

                if (!_map.TryGetChild(key, _index, out int child)) return false;

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

                return _map.GetChild(key, _index);
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
            /// Get index of the next node.
            /// </summary>
            /// <returns>Next node index if present, otherwise -1</returns>
            public int GetNext() {
                ThrowIfDisposed();

                return _map.GetNext(_index);
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

                return _map.TryGetChild(key, _index, out child);
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
            /// Get index of the next node.
            /// </summary>
            /// <param name="next">Next node index if present, otherwise -1</param>
            /// <returns>True if has next node</returns>
            public bool TryGetNext(out int next) {
                ThrowIfDisposed();

                return _map.TryGetNext(_index, out next);
            }

            /// <summary>
            /// Add child with passed key if it is not present, otherwise get.
            /// </summary>
            /// <param name="key">Key of the child</param>
            /// <returns>Added or present child node index</returns>
            public int GetOrAddChild(K key) {
                ThrowIfDisposed();

                return _map.GetOrAddChild(key, _index);
            }

            /// <summary>
            /// Remove child node by key. This operation can cause map version change.
            /// </summary>
            /// <param name="key">Key of the child</param>
            public void RemoveChild(K key) {
                ThrowIfDisposed();

                _map.RemoveChild(key, ref _index);
                _version = _map._version;
            }

            /// <summary>
            /// Check if current node contains child with passed key.
            /// </summary>
            /// <param name="key">Key of the child</param>
            /// <returns>True if contains child with passed key</returns>
            public bool ContainsChild(K key) {
                ThrowIfDisposed();

                return _map.ContainsChild(key, _index);
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

        /// <summary>
        /// Initialize storages with passed capacity of roots and nodes.
        /// </summary>
        /// <param name="roots">Capacity of roots</param>
        /// <param name="nodes">Capacity of the other nodes</param>
        public SerializedTreeMap(int roots = 0, int nodes = 0) {
            int totalCount = roots + nodes;

            _nodes = totalCount > 0 ? new Node[totalCount] : Array.Empty<Node>();
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
        /// <returns>Iterator for root, or invalid iterator if passed key is not present</returns>
        public Iterator GetTree(K key) {
            return _rootIndexMap.TryGetValue(key, out int index)
                ? new Iterator(this, index, 0, _version)
                : InvalidIterator;
        }

        /// <summary>
        /// Get iterator for root with passed key.
        /// </summary>
        /// <param name="key">Key of the root</param>
        /// <param name="iterator">Iterator for root, or invalid iterator if passed key is not present</param>
        /// <returns>True if passed root key is valid</returns>
        public bool TryGetTree(K key, out Iterator iterator) {
            iterator = GetTree(key);
            return !iterator.IsInvalid();
        }

        /// <summary>
        /// Get iterator for child with passed key and parent index.
        /// </summary>
        /// <param name="key">Key of the child</param>
        /// <param name="parent">Index of the parent node</param>
        /// <returns>Iterator for child, or invalid iterator if passed key or parent index are not present</returns>
        public Iterator GetTree(K key, int parent) {
            return _nodeIndexMap.TryGetValue(new KeyIndex(key, parent), out int index)
                ? new Iterator(this, index, GetDepth(index), _version)
                : InvalidIterator;
        }

        /// <summary>
        /// Get iterator for child with passed key and parent index.
        /// </summary>
        /// <param name="key">Key of the child</param>
        /// <param name="parent">Index of the parent node</param>
        /// <param name="iterator">Iterator for child, or invalid iterator if passed key or parent index are not present</param>
        /// <returns>True if passed child key and parent index are valid</returns>
        public bool TryGetTree(K key, int parent, out Iterator iterator) {
            iterator = GetTree(key, parent);
            return !iterator.IsInvalid();
        }

        #endregion

        #region ROOT

        /// <summary>
        /// Add root with passed key if it is not present, otherwise get.
        /// </summary>
        /// <param name="key">Key of the root</param>
        /// <returns>Added or present root node index</returns>
        public int GetOrAddRoot(K key) {
            if (_rootIndexMap.TryGetValue(key, out int index)) return index;

            index = AllocateNode(key);
            _rootIndexMap[key] = index;

            return index;
        }

        /// <summary>
        /// Get root with passed key.
        /// </summary>
        /// <param name="key">Key of the root</param>
        /// <returns>Root node index if passed key is valid, otherwise -1</returns>
        public int GetRoot(K key) {
            return _rootIndexMap.TryGetValue(key, out int index) ? index : -1;
        }

        /// <summary>
        /// Get root with passed key.
        /// </summary>
        /// <param name="key">Key of the root</param>
        /// <param name="index">Root node index if passed key is valid, otherwise -1</param>
        /// <returns>True if passed key is valid</returns>
        public bool TryGetRoot(K key, out int index) {
            index = GetRoot(key);
            return index >= 0;
        }

        /// <summary>
        /// Remove root by key. This operation can cause version change.
        /// </summary>
        /// <param name="key">Key of the root</param>
        public void RemoveRoot(K key) {
            if (!_rootIndexMap.TryGetValue(key, out int index)) return;

            DisposeNodePath(index, -1, true);
            ApplyDefragmentationIfNecessary();
        }

        /// <summary>
        /// Check if map contains root with passed key.
        /// </summary>
        /// <param name="key">Key of the root</param>
        /// <returns>True if contains</returns>
        public bool ContainsRoot(K key) {
            return _rootIndexMap.ContainsKey(key);
        }

        /// <summary>
        /// Remove all trees. This operation can cause version change.
        /// </summary>
        public void ClearAll() {
            _nodes = Array.Empty<Node>();
            _rootIndexMap.Clear();
            _nodeIndexMap.Clear();
            _freeIndices.Clear();
            _head = 0;
            _version++;
        }

        /// <summary>
        /// Force apply defragmentation to remove unused nodes. This operation can cause version change.
        /// </summary>
        public void OptimizeLayout() {
            ApplyDefragmentation();
        }

        #endregion

        #region CHILD

        /// <summary>
        /// Get index of the child node with passed key and parent index.
        /// </summary>
        /// <param name="key">Key of the child</param>
        /// <param name="parent">Index of the parent node</param>
        /// <returns>Child node index if present, otherwise -1</returns>
        public int GetChild(K key, int parent) {
            return _nodeIndexMap.TryGetValue(new KeyIndex(key, parent), out int child) ? child : -1;
        }

        /// <summary>
        /// Get index of the child node with passed key and parent index.
        /// </summary>
        /// <param name="key">Key of the child</param>
        /// <param name="parent">Index of the parent node</param>
        /// <param name="child">Child node index if present, otherwise -1</param>
        /// <returns>True if parent has child with passed key</returns>
        public bool TryGetChild(K key, int parent, out int child) {
            child = GetChild(key, parent);
            return child >= 0;
        }

        /// <summary>
        /// Add child with passed key to the parent if it is not present, otherwise get.
        /// </summary>
        /// <param name="key">Key of the child</param>
        /// <param name="parent">Index of the parent node</param>
        /// <returns>Added or present child node index</returns>
        public int GetOrAddChild(K key, int parent) {
            var nodeKey = new KeyIndex(key, parent);
            if (_nodeIndexMap.TryGetValue(nodeKey, out int index)) return index;

            if (parent < 0 || parent >= _head) return -1;

            ref var node = ref _nodes[parent];
            if (node.IsDisposed()) return -1;

            index = AllocateNode(key);
            _nodeIndexMap[nodeKey] = index;

            node = ref _nodes[parent];
            int last = node.last;

            node.last = index;

            if (last >= 0) {
                node = ref _nodes[last];
                node.next = index;
            }
            else {
                node.child = index;
                node.last = index;
                last = parent;
            }

            node = ref _nodes[index];
            node.prev = last;
            node.next = parent;

            return index;
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

            int last = node.child;
            int count = 0;

            while (last >= 0 && last != parent) {
                count++;

                node = ref _nodes[last];
                last = node.next;
            }

            return count;
        }

        /// <summary>
        /// Check if parent node contains child with passed key.
        /// </summary>
        /// <param name="key">Key of the child</param>
        /// <param name="parent">Index of the parent node</param>
        /// <returns>True if the parent node contains child with passed key</returns>
        public bool ContainsChild(K key, int parent) {
            return _nodeIndexMap.ContainsKey(new KeyIndex(key, parent));
        }

        /// <summary>
        /// Remove child node by key from parent. This operation can cause map version change.
        /// Note that parent index can change after remove during defragmentation.
        /// </summary>
        /// <param name="key">Key of the child</param>
        /// <param name="parent">Index of the parent node</param>
        public void RemoveChild(K key, int parent) {
            RemoveChild(key, ref parent);
        }

        /// <summary>
        /// Remove child node by key from parent. This operation can cause map version change.
        /// Note that parent index can change after remove during defragmentation.
        /// </summary>
        /// <param name="key">Key of the child</param>
        /// <param name="parent">Index of the parent node</param>
        public void RemoveChild(K key, ref int parent) {
            if (_nodeIndexMap.TryGetValue(new KeyIndex(key, parent), out int index)) RemoveNode(index, ref parent);
        }

        /// <summary>
        /// Remove all children from current node. This operation can cause map version change.
        /// Note that parent index can change after remove during defragmentation.
        /// </summary>
        /// <param name="parent">Index of the parent node</param>
        public void ClearChildren(int parent) {
            ClearChildren(ref parent);
        }

        /// <summary>
        /// Remove all children from current node. This operation can cause map version change.
        /// Note that parent index can change after remove during defragmentation.
        /// </summary>
        /// <param name="parent">Index of the parent node</param>
        public void ClearChildren(ref int parent) {
            if (parent < 0 || parent >= _head) return;

            ref var node = ref _nodes[parent];
            if (node.IsDisposed()) return;

            DisposeNodePath(parent, -1, false);
            parent = ApplyDefragmentationIfNecessary(parent);

            node = ref _nodes[parent];
            node.child = -1;
            node.last = -1;

            _version++;
        }

        #endregion

        #region NODE

        /// <summary>
        /// Get node by ref with passed index. Use node to get key, get and set value.
        /// Incorrect index can cause <see cref="IndexOutOfRangeException"/> or disposed node get.
        /// See if node is present by <see cref="ContainsNode"/>.
        /// </summary>
        /// <param name="index">Index of the node</param>
        /// <returns>Reference to the current node struct</returns>
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
            int prev = node.prev;

            while (prev >= 0) {
                node = ref _nodes[prev];
                if (node.child == index) depth++;

                index = prev;
                prev = node.prev;
            }

            return depth;
        }

        /// <summary>
        /// Remove child node by index from parent. This operation can cause map version change.
        /// Note that parent index can change after remove during defragmentation.
        /// </summary>
        /// <param name="index">Index of the removed node</param>
        /// <param name="parent">Index of the parent node</param>
        public void RemoveNode(int index, int parent) {
            RemoveNode(index, ref parent);
        }

        /// <summary>
        /// Remove child node by index from parent. This operation can cause map version change.
        /// Note that parent index can change after remove during defragmentation.
        /// </summary>
        /// <param name="index">Index of the removed node</param>
        /// <param name="parent">Index of the parent node</param>
        public void RemoveNode(int index, ref int parent) {
            if (index < 0 || index >= _head) {
                parent = -1;
                return;
            }

            ref var node = ref _nodes[index];
            if (node.IsDisposed()) {
                parent = -1;
                return;
            }

            int prev = node.prev;
            int next = node.next;

            DisposeNodePath(index, parent, disposeIndex: true);

            if (prev >= 0) {
                node = ref _nodes[prev];

                if (prev == next) {
                    node.child = -1;
                    node.last = -1;
                }
                else {
                    if (node.child == index) node.child = next;
                    else node.next = next;

                    node = ref _nodes[next];

                    if (node.last == index) node.last = prev;
                    else node.prev = prev;
                }
            }

            parent = ApplyDefragmentationIfNecessary(parent);
            _version++;
        }

        /// <summary>
        /// Check if node with passed index is present and not disposed.
        /// </summary>
        /// <param name="index">Index of the node</param>
        /// <returns>True if map has node with passed index and node is not disposed</returns>
        public bool ContainsNode(int index) {
            if (index < 0 || index >= _head) return false;

            ref var node = ref _nodes[index];
            return !node.IsDisposed();
        }

        #endregion

        #region CONNECTIONS

        /// <summary>
        /// Get index of node parent.
        /// </summary>
        /// <param name="child">Index of the child node</param>
        /// <returns>Parent node index if present, otherwise -1</returns>
        public int GetParent(int child) {
            if (child < 0 || child >= _head) return -1;

            ref var node = ref _nodes[child];
            if (node.IsDisposed()) return -1;

            int prev = node.prev;

            while (prev >= 0) {
                node = ref _nodes[prev];
                if (node.child == child) return prev;

                child = prev;
                prev = node.prev;
            }

            return -1;
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
        /// Get index of the next node.
        /// </summary>
        /// <param name="previous">Index of the previous node</param>
        /// <returns>Next node index if present, otherwise -1</returns>
        public int GetNext(int previous) {
            if (previous < 0 || previous >= _head) return -1;

            ref var node = ref _nodes[previous];
            if (node.IsDisposed()) return -1;

            int next = node.next;

            node = ref _nodes[next];
            if (node.last == previous) return -1;

            return next;
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

            int prev = node.prev;

            node = ref _nodes[prev];
            if (node.child == next) return -1;

            return prev;
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
        /// <param name="previous">Previous node index if present, otherwise -1</param>
        /// <returns>True if has previous node</returns>
        public bool TryGetPrevious(int next, out int previous) {
            previous = GetPrevious(next);
            return previous >= 0;
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

        private void DisposeNode(ref Node node, int index, int parent) {
            if (index == _head - 1) _head--;
            else _freeIndices.Add(index);

            if (parent < 0) _rootIndexMap.Remove(node.key);
            else _nodeIndexMap.Remove(new KeyIndex(node.key, parent));

            node.Dispose();
        }

        private void DisposeNodePath(int index, int parent, bool disposeIndex) {
            if (index < 0 || index >= _head) return;

            ref var node = ref _nodes[index];
            if (disposeIndex) DisposeNode(ref node, index, parent);

            parent = index;
            int pointer = node.child;

            while (pointer >= 0 && pointer != index) {
                node = ref _nodes[pointer];

                if (node.IsDisposed()) {
                    parent = node.last;
                }
                else {
                    DisposeNode(ref node, pointer, parent);

                    if (node.child >= 0) {
                        node.last = parent;
                        parent = pointer;
                        pointer = node.child;
                        continue;
                    }
                }

                int next = node.next;
                if (next < 0) return;

                node = ref _nodes[next];

                if (node.last == pointer) {
                    pointer = next;
                    parent = -1;
                    continue;
                }

                pointer = next;
            }
        }

        private int ApplyDefragmentationIfNecessary(int trackedIndex = -1) {
            return Count <= _nodes.Length * 0.5f ? ApplyDefragmentation(trackedIndex) : trackedIndex;
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
                int child = node.child;
                int last = node.last;
                int next = node.next;
                int prev = node.prev;

                if (child >= 0) {
                    node = ref _nodes[child];
                    node.prev = freeIndex;
                }

                if (last >= 0) {
                    node = ref _nodes[last];
                    node.next = freeIndex;
                }

                if (prev < 0) {
                    _rootIndexMap[key] = freeIndex;
                    continue;
                }

                int parent = -1;
                node = ref _nodes[prev];

                if (node.child == i) {
                    parent = prev;
                    node.child = freeIndex;
                }
                else node.next = freeIndex;

                node = ref _nodes[next];

                if (node.last == i) {
                    parent = next;
                    node.last = freeIndex;
                }
                else node.prev = freeIndex;

                if (parent < 0) {
                    child = next;
                    next = node.next;

                    while (next >= 0) {
                        node = ref _nodes[next];

                        if (node.last == child) {
                            parent = next;
                            break;
                        }

                        child = next;
                        next = node.next;
                    }
                }

                _nodeIndexMap[new KeyIndex(key, parent)] = freeIndex;
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
            sb.AppendLine($"{nameof(SerializedTreeMap<K, V>)}(version {_version}, roots {RootCount}, nodes {NodeCount})");

            foreach (var root in RootKeys) {
                sb.AppendLine(GetTree(root).ToString());
            }

            return sb.ToString();
        }
    }

}
