using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public sealed class BlueprintMeta {

        [SerializeField] private int _addedNodesTotalCount;

        [SerializeField] private IntToBlueprintNodeMetaMap _nodes;

        [SerializeField] private IntIntToListOfBlueprintLinkMap _fromNodePortLinksMap;
        [SerializeField] private IntIntToListOfBlueprintLinkMap _toNodePortLinksMap;

        [SerializeField] private List<BlueprintLink> _externalPortLinks = new List<BlueprintLink>();

        [Serializable]
        private sealed class IntToIntMap : SerializedDictionary<int, int> {}

        [Serializable]
        private sealed class IntToBlueprintNodeMetaMap : SerializedDictionary<int, BlueprintNodeMeta> {}

        [Serializable]
        private sealed class IntToBlueprintLinkMap : SerializedDictionary<int, BlueprintLink> {}

        [Serializable]
        private sealed class IntToListOfBlueprintLinkMap : SerializedDictionary<int, List<BlueprintLink>> {}

        [Serializable]
        private sealed class IntIntToListOfBlueprintLinkMap : SerializedDictionary<int, IntToListOfBlueprintLinkMap> {}

        public Action OnInvalidate;

        public Dictionary<int, BlueprintNodeMeta> Nodes => _nodes;
        public List<BlueprintLink> ExternalPortLinks => _externalPortLinks;

        public bool Invalidate() {
            bool changed = false;

            foreach (int nodeId in _nodes.Keys) {
                changed |= InvalidateNode(nodeId);
            }

            if (changed) OnInvalidate?.Invoke();

            return changed;
        }

        public void Invalidate(int nodeId) {
            if (InvalidateNode(nodeId)) OnInvalidate?.Invoke();
        }

        private bool InvalidateNode(int nodeId) {
            if (!_nodes.TryGetValue(nodeId, out var nodeMeta)) return false;

            var oldPorts = nodeMeta.Ports;
            int oldPortsCount = oldPorts.Count;

            nodeMeta.RecreatePorts();
            var newPorts = nodeMeta.Ports;
            int newPortsCount = newPorts.Count;

            bool portsChanged = oldPortsCount != newPortsCount;

            for (int oldPortIndex = 0; oldPortIndex < oldPortsCount; oldPortIndex++) {
                int oldPortSignature = oldPorts[oldPortIndex].GetSignature();
                int newPortIndex = -1;

                for (int np = 0; np < newPortsCount; np++) {
                    int newPortSignature = newPorts[np].GetSignature();
                    if (oldPortSignature != newPortSignature) continue;

                    newPortIndex = np;
                    break;
                }

                if (oldPortIndex == newPortIndex) continue;

                if (newPortIndex >= 0) {
                    SetLinksFromNodePort(nodeId, newPortIndex, GetLinksFromNodePort(nodeId, oldPortIndex));
                    SetLinksToNodePort(nodeId, newPortIndex, GetLinksToNodePort(nodeId, oldPortIndex));
                }

                RemoveLinksFromNodePort(nodeId, oldPortIndex);
                RemoveLinksToNodePort(nodeId, oldPortIndex);

                portsChanged = true;
            }

            return portsChanged;
        }

        public IReadOnlyList<BlueprintLink> GetLinksFromNodePort(int nodeId, int portIndex) {
            if (!_fromNodePortLinksMap.TryGetValue(nodeId, out var fromNodePortLinksMap) ||
                !fromNodePortLinksMap.TryGetValue(portIndex, out var fromNodePortLinks)
            ) {
                return Array.Empty<BlueprintLink>();
            }

            return fromNodePortLinks;
        }

        public IReadOnlyList<BlueprintLink> GetLinksToNodePort(int nodeId, int portIndex) {
            if (!_toNodePortLinksMap.TryGetValue(nodeId, out var toNodePortLinksMap) ||
                !toNodePortLinksMap.TryGetValue(portIndex, out var toNodePortLinks)
               ) {
                return Array.Empty<BlueprintLink>();
            }

            return toNodePortLinks;
        }

        private void SetLinksFromNodePort(int nodeId, int portIndex, IReadOnlyList<BlueprintLink> links) {
            if (!_fromNodePortLinksMap.TryGetValue(nodeId, out var fromNodePortLinksMap)) {
                fromNodePortLinksMap = new IntToListOfBlueprintLinkMap();
                _fromNodePortLinksMap[nodeId] = fromNodePortLinksMap;
            }

            fromNodePortLinksMap[portIndex] = new List<BlueprintLink>(links);
        }

        private void SetLinksToNodePort(int nodeId, int portIndex, IReadOnlyList<BlueprintLink> links) {
            if (!_toNodePortLinksMap.TryGetValue(nodeId, out var toNodePortLinksMap)) {
                toNodePortLinksMap = new IntToListOfBlueprintLinkMap();
                _toNodePortLinksMap[nodeId] = toNodePortLinksMap;
            }

            toNodePortLinksMap[portIndex] = new List<BlueprintLink>(links);
        }

        public void AddNode(BlueprintNodeMeta nodeMeta) {
            int nodeId = _addedNodesTotalCount++;

            nodeMeta.NodeId = nodeId;

            _nodes.Add(nodeId, nodeMeta);
            FetchExternalPorts(nodeId, nodeMeta);
        }

        public void RemoveNode(int nodeId) {
            if (!_nodes.ContainsKey(nodeId)) return;

            RemoveLinksFromNode(nodeId);
            RemoveLinksToNode(nodeId);

            RemoveRelatedExternalPorts(nodeId);

            _nodes.Remove(nodeId);
        }

        public bool TryCreateConnection(int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
            if (fromNodeId == toNodeId) return false;

            if (!_nodes.TryGetValue(fromNodeId, out var fromNode)) return false;
            if (!_nodes.TryGetValue(toNodeId, out var toNode)) return false;

            if (fromPortIndex < 0 || fromPortIndex > fromNode.Ports.Count - 1) return false;
            if (toPortIndex < 0 || toPortIndex > toNode.Ports.Count - 1) return false;

            if (HasLinkFromNodePort(fromNodeId, fromPortIndex, toNodeId, toPortIndex)) return false;
            if (HasLinkToNodePort(fromNodeId, fromPortIndex, toNodeId, toPortIndex)) return false;

            var fromPort = fromNode.Ports[fromPortIndex];
            var toPort = toNode.Ports[toPortIndex];

            // fromPort is an enter port
            if (!fromPort.isDataPort && !fromPort.isExitPort) {
                // toPort must be an exit port
                if (toPort.isDataPort || !toPort.isExitPort) return false;

                // adding connection from the exit port toPort to the enter port fromPort
                CreateConnection(
                    toNodeId, toPortIndex, toPort.GetSignature(),
                    fromNodeId, fromPortIndex, fromPort.GetSignature()
                );
                return true;
            }

            // fromPort is an exit port
            if (!fromPort.isDataPort) {
                // toPort must be an enter port
                if (toPort.isDataPort || toPort.isExitPort) return false;

                // adding connection from the exit port fromPort to the enter port toPort
                CreateConnection(
                    fromNodeId, fromPortIndex, fromPort.GetSignature(),
                    toNodeId, toPortIndex, toPort.GetSignature()
                );
                return true;
            }

            // fromPort is an input port
            if (!fromPort.isExitPort) {
                // toPort must be an output port
                if (!toPort.isDataPort || !toPort.isExitPort) return false;

                // input and output must have same data type
                if (fromPort.hasDataType && toPort.hasDataType &&
                    fromPort.DataType != toPort.DataType) return false;

                // replacing connections from the input port fromPort to the output port toPort with new connection
                RemoveLinksFromNodePort(fromNodeId, fromPortIndex);
                CreateConnection(
                    fromNodeId, fromPortIndex, fromPort.GetSignature(),
                    toNodeId, toPortIndex, toPort.GetSignature()
                );
                return true;
            }

            // fromPort is an output port
            // toPort must be an input port
            if (!toPort.isDataPort || toPort.isExitPort) return false;

            // input and output must have same data type
            if (fromPort.hasDataType && toPort.hasDataType &&
                fromPort.DataType != toPort.DataType) return false;

            // replacing connections from the input port toPort to the output port fromPort with new connection
            RemoveLinksFromNodePort(toNodeId, toPortIndex);
            CreateConnection(
                toNodeId, toPortIndex, toPort.GetSignature(),
                fromNodeId, fromPortIndex, fromPort.GetSignature()
            );
            return true;
        }

        public void RemoveConnection(int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
            RemoveLinkFromNodePort(fromNodeId, fromPortIndex, toNodeId, toPortIndex);
            RemoveLinkToNodePort(fromNodeId, fromPortIndex, toNodeId, toPortIndex);
        }

        public void Clear() {
            _nodes.Clear();
            _addedNodesTotalCount = 0;

            _fromNodePortLinksMap.Clear();
            _toNodePortLinksMap.Clear();
        }

        private void FetchExternalPorts(int nodeId, BlueprintNodeMeta nodeMeta) {
            var ports = nodeMeta.Ports;
            for (int p = 0; p < ports.Count; p++) {
                var port = ports[p];
                if (!port.isExternalPort) continue;

                _externalPortLinks.Add(new BlueprintLink { nodeId = nodeId, portIndex = p });
            }
        }

        private void RemoveRelatedExternalPorts(int nodeId) {
            for (int i = _externalPortLinks.Count - 1; i >= 0; i--) {
                if (_externalPortLinks[i].nodeId == nodeId) _externalPortLinks.RemoveAt(i);
            }
        }

        private void CreateConnection(
            int fromNodeId, int fromPortIndex, int fromPortSignature,
            int toNodeId, int toPortIndex, int toPortSignature
        ) {
            AddLinkFromNodePort(fromNodeId, fromPortIndex, toNodeId, toPortIndex, toPortSignature);
            AddLinkToNodePort(fromNodeId, fromPortIndex, fromPortSignature, toNodeId, toPortIndex);
        }

        private bool HasLinkFromNodePort(int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
            if (!_fromNodePortLinksMap.TryGetValue(fromNodeId, out var fromNodePortLinksMap)) return false;
            if (!fromNodePortLinksMap.TryGetValue(fromPortIndex, out var fromNodePortLinks)) return false;

            for (int i = 0; i < fromNodePortLinks.Count; i++) {
                var link = fromNodePortLinks[i];
                if (link.nodeId == toNodeId && link.portIndex == toPortIndex) return true;
            }

            return false;
        }

        private bool HasLinkToNodePort(int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
            if (!_toNodePortLinksMap.TryGetValue(toNodeId, out var toNodePortLinksMap)) return false;
            if (!toNodePortLinksMap.TryGetValue(toPortIndex, out var toNodePortLinks)) return false;

            for (int i = 0; i < toNodePortLinks.Count; i++) {
                var link = toNodePortLinks[i];
                if (link.nodeId == fromNodeId && link.portIndex == fromPortIndex) return true;
            }

            return false;
        }

        private void AddLinkFromNodePort(int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex, int toPortSignature) {
            if (!_fromNodePortLinksMap.TryGetValue(fromNodeId, out var fromNodePortLinksMap)) {
                fromNodePortLinksMap = new IntToListOfBlueprintLinkMap();
                _fromNodePortLinksMap[fromNodeId] = fromNodePortLinksMap;
            }

            if (!fromNodePortLinksMap.TryGetValue(fromPortIndex, out var fromNodePortLinks)) {
                fromNodePortLinks = new List<BlueprintLink>();
                fromNodePortLinksMap[fromPortIndex] = fromNodePortLinks;
            }

            var link = new BlueprintLink { nodeId = toNodeId, portIndex = toPortIndex, portSignature = toPortSignature };
            fromNodePortLinks.Add(link);
        }

        private void AddLinkToNodePort(int fromNodeId, int fromPortIndex, int fromPortSignature, int toNodeId, int toPortIndex) {
            if (!_toNodePortLinksMap.TryGetValue(toNodeId, out var toNodePortLinksMap)) {
                toNodePortLinksMap = new IntToListOfBlueprintLinkMap();
                _toNodePortLinksMap[toNodeId] = toNodePortLinksMap;
            }

            if (!toNodePortLinksMap.TryGetValue(toPortIndex, out var toNodePortLinks)) {
                toNodePortLinks = new List<BlueprintLink>();
                toNodePortLinksMap[toPortIndex] = toNodePortLinks;
            }

            var link = new BlueprintLink { nodeId = fromNodeId, portIndex = fromPortIndex, portSignature = fromPortSignature };
            toNodePortLinks.Add(link);
        }

        private void RemoveLinkFromNodePort(int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
            if (!_fromNodePortLinksMap.TryGetValue(fromNodeId, out var portLinksMap)) return;
            if (!portLinksMap.TryGetValue(fromPortIndex, out var portLinks)) return;

            for (int i = 0; i < portLinks.Count; i++) {
                var link = portLinks[i];
                if (link.nodeId != toNodeId || link.portIndex != toPortIndex) continue;

                portLinks.RemoveAt(i);
            }

            if (portLinks.Count == 0) portLinksMap.Remove(fromPortIndex);
            if (portLinksMap.Count == 0) _fromNodePortLinksMap.Remove(fromNodeId);
        }

        private void RemoveLinkToNodePort(int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
            if (!_toNodePortLinksMap.TryGetValue(toNodeId, out var portLinksMap)) return;
            if (!portLinksMap.TryGetValue(toPortIndex, out var portLinks)) return;

            for (int i = 0; i < portLinks.Count; i++) {
                var link = portLinks[i];
                if (link.nodeId != fromNodeId || link.portIndex != fromPortIndex) continue;

                portLinks.RemoveAt(i);
            }

            if (portLinks.Count == 0) portLinksMap.Remove(toPortIndex);
            if (portLinksMap.Count == 0) _toNodePortLinksMap.Remove(toNodeId);
        }

        private void RemoveLinksToNodePort(int nodeId, int portIndex) {
            if (!_toNodePortLinksMap.TryGetValue(nodeId, out var portLinksMap)) return;
            if (!portLinksMap.TryGetValue(portIndex, out var links)) return;

            for (int i = 0; i < links.Count; i++) {
                var link = links[i];

                if (!_fromNodePortLinksMap.TryGetValue(link.nodeId, out var linkedNodePortLinksMap)) continue;
                if (!linkedNodePortLinksMap.TryGetValue(link.portIndex, out var linkedPortLinks)) continue;

                for (int j = 0; j < linkedPortLinks.Count; j++) {
                    var linkedPortLink = linkedPortLinks[j];
                    if (linkedPortLink.nodeId != nodeId || linkedPortLink.portIndex != portIndex) continue;

                    linkedPortLinks.RemoveAt(j);
                }

                if (linkedPortLinks.Count == 0) linkedNodePortLinksMap.Remove(link.portIndex);
                if (linkedNodePortLinksMap.Count == 0) _fromNodePortLinksMap.Remove(link.nodeId);
            }

            portLinksMap.Remove(portIndex);
            if (portLinksMap.Count == 0) _toNodePortLinksMap.Remove(nodeId);
        }

        private void RemoveLinksFromNodePort(int nodeId, int portIndex) {
            if (!_fromNodePortLinksMap.TryGetValue(nodeId, out var portLinksMap)) return;
            if (!portLinksMap.TryGetValue(portIndex, out var links)) return;

            for (int i = 0; i < links.Count; i++) {
                var link = links[i];

                if (!_toNodePortLinksMap.TryGetValue(link.nodeId, out var linkedNodePortLinksMap)) continue;
                if (!linkedNodePortLinksMap.TryGetValue(link.portIndex, out var linkedPortLinks)) continue;

                for (int j = 0; j < linkedPortLinks.Count; j++) {
                    var linkedPortLink = linkedPortLinks[j];
                    if (linkedPortLink.nodeId != nodeId || linkedPortLink.portIndex != portIndex) continue;

                    linkedPortLinks.RemoveAt(j);
                }

                if (linkedPortLinks.Count == 0) linkedNodePortLinksMap.Remove(link.portIndex);
                if (linkedNodePortLinksMap.Count == 0) _toNodePortLinksMap.Remove(link.nodeId);
            }

            portLinksMap.Remove(portIndex);
            if (portLinksMap.Count == 0) _fromNodePortLinksMap.Remove(nodeId);
        }

        private void RemoveLinksFromNode(int nodeId) {
            if (!_fromNodePortLinksMap.TryGetValue(nodeId, out var portLinksMap)) return;

            foreach ((int _, var links) in portLinksMap) {
                for (int i = 0; i < links.Count; i++) {
                    var link = links[i];

                    if (!_toNodePortLinksMap.TryGetValue(link.nodeId, out var linkedNodePortLinksMap)) continue;
                    if (!linkedNodePortLinksMap.TryGetValue(link.portIndex, out var linkedPortLinks)) continue;

                    for (int j = 0; j < linkedPortLinks.Count; j++) {
                        var linkedPortLink = linkedPortLinks[j];
                        if (linkedPortLink.nodeId != nodeId) continue;

                        linkedPortLinks.RemoveAt(j);
                    }

                    if (linkedPortLinks.Count == 0) linkedNodePortLinksMap.Remove(link.portIndex);
                    if (linkedNodePortLinksMap.Count == 0) _toNodePortLinksMap.Remove(link.nodeId);
                }
            }

            _fromNodePortLinksMap.Remove(nodeId);
        }

        private void RemoveLinksToNode(int nodeId) {
            if (!_toNodePortLinksMap.TryGetValue(nodeId, out var portLinksMap)) return;

            foreach ((int _, var links) in portLinksMap) {
                for (int i = 0; i < links.Count; i++) {
                    var link = links[i];

                    if (!_fromNodePortLinksMap.TryGetValue(link.nodeId, out var linkedNodePortLinksMap)) continue;
                    if (!linkedNodePortLinksMap.TryGetValue(link.portIndex, out var linkedPortLinks)) continue;

                    for (int j = 0; j < linkedPortLinks.Count; j++) {
                        var linkedPortLink = linkedPortLinks[j];
                        if (linkedPortLink.nodeId != nodeId) continue;

                        linkedPortLinks.RemoveAt(j);
                    }

                    if (linkedPortLinks.Count == 0) linkedNodePortLinksMap.Remove(link.portIndex);
                    if (linkedNodePortLinksMap.Count == 0) _fromNodePortLinksMap.Remove(link.nodeId);
                }
            }

            _toNodePortLinksMap.Remove(nodeId);
        }
    }

}
