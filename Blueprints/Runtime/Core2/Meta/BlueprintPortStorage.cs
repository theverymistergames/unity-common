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

        public TreeMap<int, int> CreatePortSignatureToIndicesTree(long id) {
            BlueprintNodeAddress.Unpack(id, out int factoryId, out int nodeId);

            if (!_portTree.TryGetIndex(factoryId, out int factoryRoot) ||
                !_portTree.TryGetIndex(nodeId, factoryRoot, out int nodeRoot) ||
                !_portTree.TryGetChildIndex(nodeRoot, out int p)
            ) {
                return null;
            }

            var treeMap = new TreeMap<int, int>();

            while (p >= 0) {
                int index = _portTree.GetKeyAt(p);
                int sign = _portTree.GetValueAt(p).GetSignature();

                treeMap.GetOrAddNode(index, treeMap.GetOrAddNode(sign));

                _portTree.TryGetNextIndex(p, out p);
            }

            return treeMap;
        }

        public Port GetPort(int index) {
            return _portTree.GetValueAt(index);
        }

        public int GetPortKey(int index) {
            return _portTree.GetKeyAt(index);
        }

        public int GetPortCount(long id) {
            BlueprintNodeAddress.Unpack(id, out int factoryId, out int nodeId);

            if (!_portTree.TryGetIndex(factoryId, out int factoryRoot) ||
                !_portTree.TryGetIndex(nodeId, factoryRoot, out int nodeRoot)
            ) {
                return 0;
            }

            return _portTree.GetChildrenCount(nodeRoot);
        }

        public bool TryGetPorts(long id, out int firstPort) {
            BlueprintNodeAddress.Unpack(id, out int factoryId, out int nodeId);
            firstPort = -1;

            return _portTree.TryGetIndex(factoryId, out int factoryRoot) &&
                   _portTree.TryGetIndex(nodeId, factoryRoot, out int nodeRoot) &&
                   _portTree.TryGetChildIndex(nodeRoot, out firstPort);
        }

        public bool TryGetNextPortIndex(int previousPort, out int nextPort) {
            return _portTree.TryGetNextIndex(previousPort, out nextPort);
        }

        public bool TryGetPortIndex(long id, int index, out int port) {
            BlueprintNodeAddress.Unpack(id, out int factoryId, out int nodeId);
            port = -1;

            return _portTree.TryGetIndex(factoryId, out int factoryRoot) &&
                   _portTree.TryGetIndex(nodeId, factoryRoot, out int nodeRoot) &&
                   _portTree.TryGetIndex(index, nodeRoot, out port);
        }

        public void AddPort(long id, Port port) {
            BlueprintNodeAddress.Unpack(id, out int factoryId, out int nodeId);

            int factoryRoot = _portTree.GetOrAddNode(factoryId);
            int nodeRoot = _portTree.GetOrAddNode(nodeId, factoryRoot);
            int count = _portTree.GetChildrenCount(nodeRoot);

            int portRoot = _portTree.GetOrAddNode(count, nodeRoot);
            _portTree.SetValueAt(portRoot, port);
        }

        public void RemoveNode(long id) {
            BlueprintNodeAddress.Unpack(id, out int factoryId, out int nodeId);

            if (!_portTree.TryGetIndex(factoryId, out int factoryRoot) ||
                !_portTree.TryGetIndex(nodeId, factoryRoot, out int nodeRoot)
            ) {
                return;
            }

            _portTree.RemoveNodeAt(nodeRoot);
        }

        public void Clear() {
            _portTree.Clear();
        }
    }

}
