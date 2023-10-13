using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public sealed class BlueprintPortStorage {

        [SerializeField] private TreeMap<int, Port> _portTree;

        public Port GetPortData(int index) {
            return _portTree.GetValueAt(index);
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

        public bool TryGetNextPort(int previousPort, out int nextPort) {
            return _portTree.TryGetNextIndex(previousPort, out nextPort);
        }

        public bool TryGetPort(long id, int index, out int port) {
            BlueprintNodeAddress.Unpack(id, out int factoryId, out int nodeId);
            port = -1;

            return _portTree.TryGetIndex(factoryId, out int factoryRoot) &&
                   _portTree.TryGetIndex(nodeId, factoryRoot, out int nodeRoot) &&
                   _portTree.TryGetIndex(index, nodeRoot, out port);
        }

        public void AddPort(long id, int index, Port port) {
            BlueprintNodeAddress.Unpack(id, out int factoryId, out int nodeId);

            int factoryRoot = _portTree.GetOrAddNode(factoryId);
            int nodeRoot = _portTree.GetOrAddNode(nodeId, factoryRoot);
            int portRoot = _portTree.GetOrAddNode(index, nodeRoot);

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
