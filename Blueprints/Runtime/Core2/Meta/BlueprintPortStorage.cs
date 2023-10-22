using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public sealed class BlueprintPortStorage {

        [SerializeField] private TreeMap<int, Port> _portTree;

        public BlueprintPortStorage(int capacity = 0) {
            _portTree = new TreeMap<int, Port>(capacity);
        }

        public TreeMap<int, int> CreatePortSignatureToIndicesTree(NodeId id) {
            if (!_portTree.TryGetNode(id.source, out int sourceRoot) ||
                !_portTree.TryGetNode(id.node, sourceRoot, out int nodeRoot) ||
                !_portTree.TryGetChild(nodeRoot, out int p)
            ) {
                return null;
            }

            var treeMap = new TreeMap<int, int>();

            while (p >= 0) {
                int index = _portTree.GetKeyAt(p);
                int sign = _portTree.GetValueAt(p).GetSignature();

                treeMap.GetOrAddNode(index, treeMap.GetOrAddNode(sign));

                _portTree.TryGetNext(p, out p);
            }

            return treeMap;
        }

        public bool TryGetPort(NodeId id, int index, out Port port) {
            if (!_portTree.TryGetNode(id.source, out int sourceRoot) ||
                !_portTree.TryGetNode(id.node, sourceRoot, out int nodeRoot) ||
                !_portTree.TryGetNode(index, nodeRoot, out int portRoot)
            ) {
                port = default;
                return false;
            }

            port = _portTree.GetValueAt(portRoot);
            return true;
        }

        public int GetPortCount(NodeId id) {
            if (!_portTree.TryGetNode(id.source, out int sourceRoot) ||
                !_portTree.TryGetNode(id.node, sourceRoot, out int nodeRoot)
            ) {
                return 0;
            }

            return _portTree.GetChildrenCount(nodeRoot);
        }

        public void AddPort(NodeId id, Port port) {
            int sourceRoot = _portTree.GetOrAddNode(id.source);
            int nodeRoot = _portTree.GetOrAddNode(id.node, sourceRoot);
            int count = _portTree.GetChildrenCount(nodeRoot);

            int portRoot = _portTree.GetOrAddNode(count, nodeRoot);
            _portTree.SetValueAt(portRoot, port);
        }

        public void RemoveNode(NodeId id) {
            if (!_portTree.TryGetNode(id.source, out int sourceRoot) ||
                !_portTree.TryGetNode(id.node, sourceRoot, out int nodeRoot)
            ) {
                return;
            }

            _portTree.RemoveNodeAt(nodeRoot);
        }

        public void Clear() {
            _portTree.Clear();
        }

        public override string ToString() {
            return $"{nameof(BlueprintPortStorage)}: ports {_portTree}";
        }
    }

}
