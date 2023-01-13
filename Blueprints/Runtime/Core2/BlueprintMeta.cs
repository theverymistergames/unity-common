using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public sealed class BlueprintMeta {

        [SerializeField] private int _addedNodesTotalCount;
        [SerializeField] private int _addedConnectionsTotalCount;

        [SerializeField] private IntToBlueprintNodeMetaMap _nodes;
        [SerializeField] private IntToBlueprintConnectionMap _connections;

        [SerializeField] private IntToListIntMap _fromNodeConnectionIdsMap;
        [SerializeField] private IntToListIntMap _toNodeConnectionIdsMap;

        [Serializable] private sealed class IntToBlueprintNodeMetaMap : SerializedDictionary<int, BlueprintNodeMeta> {}
        [Serializable] private sealed class IntToBlueprintConnectionMap : SerializedDictionary<int, BlueprintConnection> {}
        [Serializable] private sealed class IntToListIntMap : SerializedDictionary<int, List<int>> {}

        public IReadOnlyDictionary<int, BlueprintNodeMeta> Nodes => _nodes;
        public IReadOnlyDictionary<int, BlueprintConnection> Connections => _connections;

        public int AddNode(BlueprintNodeMeta nodeMeta) {
            int nodeId = _addedNodesTotalCount++;

            _nodes.Add(nodeId, nodeMeta);

            return nodeId;
        }

        public void RemoveNode(int nodeId) {
            if (!_nodes.ContainsKey(nodeId)) return;

            RemoveConnectionsFromNode(nodeId);
            RemoveConnectionsToNode(nodeId);

            _nodes.Remove(nodeId);
        }

        public bool TryConnectNodes(
            int fromNodeId, int fromPortIndex,
            int toNodeId, int toPortIndex,
            out int connectionId
        ) {
            connectionId = -1;

            if (fromNodeId == toNodeId) return false;

            if (!_nodes.TryGetValue(fromNodeId, out var fromNode)) return false;
            if (!_nodes.TryGetValue(toNodeId, out var toNode)) return false;

            if (fromPortIndex < 0 || fromPortIndex > fromNode.Ports.Count - 1) return false;
            if (toPortIndex < 0 || toPortIndex > toNode.Ports.Count - 1) return false;

            var fromPort = fromNode.Ports[fromPortIndex];
            var toPort = toNode.Ports[toPortIndex];

            // fromPort is an enter port
            if (!fromPort.isDataPort && !fromPort.isExitPort) {
                // toPort must be an exit port
                if (toPort.isDataPort || !toPort.isExitPort) return false;

                // adding connection from the exit port toPort to the enter port fromPort
                return TryCreateConnection(
                    toNodeId, toPortIndex, fromPort.GetHashCode(),
                    fromNodeId, fromPortIndex, toPort.GetHashCode(),
                    out connectionId
                );
            }

            // fromPort is an exit port
            if (!fromPort.isDataPort) {
                // toPort must be an enter port
                if (toPort.isDataPort || toPort.isExitPort) return false;

                // adding connection from the exit port fromPort to the enter port toPort
                return TryCreateConnection(
                    fromNodeId, fromPortIndex, toPort.GetHashCode(),
                    toNodeId, toPortIndex, fromPort.GetHashCode(),
                    out connectionId
                );
            }

            // fromPort is an input port
            if (!fromPort.isExitPort) {
                // toPort must be an output port
                if (!toPort.isDataPort || !toPort.isExitPort) return false;

                // input and output must have same data type
                if (fromPort.hasDataType && toPort.hasDataType &&
                    fromPort.dataTypeHash != toPort.dataTypeHash) return false;

                // replacing connections from the input port fromPort to the output port toPort with new connection
                RemoveConnectionsFromNodePort(fromNodeId, fromPortIndex);
                return TryCreateConnection(
                    fromNodeId, fromPortIndex, toPort.GetHashCode(),
                    toNodeId, toPortIndex, fromPort.GetHashCode(),
                    out connectionId
                );
            }

            // fromPort is an output port
            // toPort must be an input port
            if (!toPort.isDataPort || toPort.isExitPort) return false;

            // input and output must have same data type
            if (fromPort.hasDataType && toPort.hasDataType &&
                fromPort.dataTypeHash != toPort.dataTypeHash) return false;

            // replacing connections from the input port toPort to the output port fromPort with new connection
            RemoveConnectionsFromNodePort(toNodeId, toPortIndex);
            return TryCreateConnection(
                toNodeId, toPortIndex, fromPort.GetHashCode(),
                fromNodeId, fromPortIndex, toPort.GetHashCode(),
                out connectionId
            );
        }

        public void RemoveConnection(int connectionId) {
            if (!_connections.TryGetValue(connectionId, out var connection)) return;

            _connections.Remove(connectionId);

            RemoveConnectionsFromNodePort(connection.fromNodeId, connection.fromPortIndex);
            RemoveConnectionsToNodePort(connection.toNodeId, connection.toPortIndex);
        }

        public int GetNodePortConnectionsCount(int nodeId, int portIndex) {
            if (!_fromNodeConnectionIdsMap.TryGetValue(nodeId, out var connectionIds)) return 0;

            int count = 0;
            for (int i = 0; i < connectionIds.Count; i++) {
                var c = _connections[connectionIds[i]];
                if (c.fromNodeId == nodeId && c.fromPortIndex == portIndex) count++;
            }

            return count;
        }

        public int GetNodePortConnectionId(int nodeId, int portIndex, int linkIndex) {
            if (!_fromNodeConnectionIdsMap.TryGetValue(nodeId, out var connectionIds)) return -1;

            int count = 0;
            for (int i = 0; i < connectionIds.Count; i++) {
                int connectionId = connectionIds[i];

                var c = _connections[connectionId];
                if (c.fromNodeId != nodeId || c.fromPortIndex != portIndex || linkIndex != count++) continue;

                return connectionId;
            }

            return -1;
        }

        public void Clear() {
            _nodes.Clear();
            _connections.Clear();

            _fromNodeConnectionIdsMap.Clear();
            _toNodeConnectionIdsMap.Clear();

            _addedNodesTotalCount = 0;
            _addedConnectionsTotalCount = 0;
        }

        public void Invalidate() {
            foreach ((int nodeId, var nodeMeta) in _nodes) {
                nodeMeta.RecreatePorts();
            }

            foreach ((int connectionId, var connection) in _connections) {
                connection.fromNodeId
            }
        }

        public void InvalidateNode(int nodeId) {
            if (!_nodes.TryGetValue(nodeId, out var nodeMeta)) return;

            nodeMeta.RecreatePorts();

            if (_fromNodeConnectionIdsMap.TryGetValue(nodeId, out var fromNodeConnectionIds)) {
                for (int i = 0; i < fromNodeConnectionIds.Count; i++) {
                    int connectionId = fromNodeConnectionIds[i];
                    var connection = _connections[connectionId];


                }
            }
        }

        public void OnValidate(BlueprintAsset owner) {
            foreach ((int nodeId, var nodeMeta) in _nodes) {
                nodeMeta.OnValidate(nodeId, owner);
            }
        }

        private bool TryCreateConnection(
            int fromNodeId, int fromPortIndex, int fromPortHash,
            int toNodeId, int toPortIndex, int toPortHash,
            out int connectionId
        ) {
            connectionId = -1;
            if (HasConnection(fromNodeId, fromPortIndex, toNodeId, toPortIndex)) return false;

            connectionId = _addedConnectionsTotalCount++;

            _connections.Add(connectionId, new BlueprintConnection {
                fromNodeId = fromNodeId,
                fromPortIndex = fromPortIndex,
                fromPortHash = fromPortHash,
                toNodeId = toNodeId,
                toPortIndex = toPortIndex,
                toPortHash = toPortHash,
            });

            AddConnectionFromNode(fromNodeId, connectionId);
            AddConnectionToNode(toNodeId, connectionId);

            return true;
        }

        private bool HasConnection(
            int fromNodeId, int fromPortIndex,
            int toNodeId, int toPortIndex
        ) {
            if (!_fromNodeConnectionIdsMap.TryGetValue(fromNodeId, out var connectionIds)) return false;

            for (int i = 0; i < connectionIds.Count; i++) {
                int id = connectionIds[i];
                if (_connections.TryGetValue(id, out var connection) &&
                    connection.fromNodeId == fromNodeId &&
                    connection.fromPortIndex == fromPortIndex &&
                    connection.toNodeId == toNodeId &&
                    connection.toPortIndex == toPortIndex
                ) {
                    return true;
                }
            }

            return false;
        }

        private void AddConnectionFromNode(int fromNodeId, int connectionId) {
            if (_fromNodeConnectionIdsMap.TryGetValue(fromNodeId, out var ids)) {
                ids.Add(connectionId);
                return;
            }

            _fromNodeConnectionIdsMap[fromNodeId] = new List<int>(connectionId);
        }

        private void AddConnectionToNode(int toNodeId, int connectionId) {
            if (_toNodeConnectionIdsMap.TryGetValue(toNodeId, out var ids)) {
                ids.Add(connectionId);
                return;
            }

            _toNodeConnectionIdsMap[toNodeId] = new List<int>(connectionId);
        }

        private void RemoveConnectionsFromNodePort(int nodeId, int portIndex) {
            if (!_fromNodeConnectionIdsMap.TryGetValue(nodeId, out var ids)) return;

            for (int i = ids.Count - 1; i >= 0; i--) {
                int id = ids[i];

                if (!_connections.TryGetValue(id, out var connection)) {
                    ids.RemoveAt(i);
                    continue;
                }

                if (connection.fromNodeId != nodeId || connection.fromPortIndex != portIndex) {
                    continue;
                }

                _connections.Remove(id);
                ids.RemoveAt(i);
            }

            if (ids.Count == 0) _fromNodeConnectionIdsMap.Remove(nodeId);
        }

        private void RemoveConnectionsToNodePort(int nodeId, int portIndex) {
            if (!_toNodeConnectionIdsMap.TryGetValue(nodeId, out var ids)) return;

            for (int i = ids.Count - 1; i >= 0; i--) {
                int id = ids[i];

                if (!_connections.TryGetValue(id, out var connection)) {
                    ids.RemoveAt(i);
                    continue;
                }

                if (connection.toNodeId != nodeId || connection.toPortIndex != portIndex) {
                    continue;
                }

                _connections.Remove(id);
                ids.RemoveAt(i);
            }

            if (ids.Count == 0) _toNodeConnectionIdsMap.Remove(nodeId);
        }

        private void RemoveConnectionsFromNode(int nodeId) {
            if (!_fromNodeConnectionIdsMap.TryGetValue(nodeId, out var ids)) return;

            for (int i = 0; i < ids.Count; i++) {
                _connections.Remove(ids[i]);
            }

            _fromNodeConnectionIdsMap.Remove(nodeId);
        }

        private void RemoveConnectionsToNode(int nodeId) {
            if (!_toNodeConnectionIdsMap.TryGetValue(nodeId, out var ids)) return;

            for (int i = 0; i < ids.Count; i++) {
                _connections.Remove(ids[i]);
            }

            _toNodeConnectionIdsMap.Remove(nodeId);
        }
    }

}
