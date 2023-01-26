﻿using System;
using System.Collections.Generic;
using System.Text;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Meta {

    [Serializable]
    public sealed class BlueprintMeta {

        [SerializeField] private int _addedNodesTotalCount;
        [SerializeField] private SerializedDictionary<int, BlueprintNodeMeta> _nodesMap;
        [SerializeField] private SerializedDictionary<int, SerializedDictionary<int, List<BlueprintLink>>> _fromNodePortLinksMap;
        [SerializeField] private SerializedDictionary<int, SerializedDictionary<int, List<BlueprintLink>>> _toNodePortLinksMap;
        [SerializeField] private SerializedDictionary<int, BlueprintAsset> _subgraphReferencesMap;

        public Action<int> OnInvalidateNodePortsAndLinks;

        public Dictionary<int, BlueprintNodeMeta> NodesMap => _nodesMap;
        public Dictionary<int, BlueprintAsset> SubgraphReferencesMap => _subgraphReferencesMap;

        public void AddNode(BlueprintNodeMeta  nodeMeta) {
            int nodeId = _addedNodesTotalCount++;
            nodeMeta.NodeId = nodeId;

            _nodesMap.Add(nodeId, nodeMeta);
        }

        public void RemoveNode(int nodeId) {
            if (!_nodesMap.ContainsKey(nodeId)) return;

            RemoveLinksFromNode(nodeId);
            RemoveLinksToNode(nodeId);

            _nodesMap.Remove(nodeId);
        }

        public bool TryCreateConnection(int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
            if (fromNodeId == toNodeId) return false;

            if (!_nodesMap.TryGetValue(fromNodeId, out var fromNode)) return false;
            if (!_nodesMap.TryGetValue(toNodeId, out var toNode)) return false;

            if (fromPortIndex < 0 || fromPortIndex > fromNode.Ports.Length - 1) return false;
            if (toPortIndex < 0 || toPortIndex > toNode.Ports.Length - 1) return false;

            if (HasLinkFromNodePort(fromNodeId, fromPortIndex, toNodeId, toPortIndex)) return false;
            if (HasLinkToNodePort(fromNodeId, fromPortIndex, toNodeId, toPortIndex)) return false;

            var fromPort = fromNode.Ports[fromPortIndex];
            var toPort = toNode.Ports[toPortIndex];

            if (fromPort.mode == Port.Mode.Enter) {
                if (toPort.mode != Port.Mode.Exit) return false;

                // adding connection from the exit port toPort to the enter port fromPort
                CreateConnection(toNodeId, toPortIndex, fromNodeId, fromPortIndex);
                return true;
            }

            if (fromPort.mode == Port.Mode.Exit) {
                if (toPort.mode != Port.Mode.Enter) return false;

                // adding connection from the exit port fromPort to the enter port toPort
                CreateConnection(fromNodeId, fromPortIndex, toNodeId, toPortIndex);
                return true;
            }

            if (fromPort.mode is Port.Mode.Input or Port.Mode.NonTypedInput) {
                if (toPort.mode is not (Port.Mode.Output or Port.Mode.NonTypedOutput)) return false;

                // input and output must have same data type
                if (fromPort.mode == Port.Mode.Input &&
                    toPort.mode == Port.Mode.Output &&
                    fromPort.DataType != toPort.DataType) return false;

                // replacing connections from the input port fromPort to the output port toPort with new connection
                RemoveLinksFromNodePort(fromNodeId, fromPortIndex);
                CreateConnection(fromNodeId, fromPortIndex, toNodeId, toPortIndex);
                return true;
            }

            if (fromPort.mode is Port.Mode.Output or Port.Mode.NonTypedOutput) {
                if (toPort.mode is not (Port.Mode.Input or Port.Mode.NonTypedInput)) return false;

                // input and output must have same data type
                if (fromPort.mode == Port.Mode.Output &&
                    toPort.mode == Port.Mode.Input &&
                    fromPort.DataType != toPort.DataType) return false;

                // replacing connections from the input port fromPort to the output port toPort with new connection
                RemoveLinksFromNodePort(toNodeId, toPortIndex);
                CreateConnection(toNodeId, toPortIndex, fromNodeId, fromPortIndex);
                return true;
            }

            return false;
        }

        public void RemoveConnection(int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
            RemoveLinkFromNodePort(fromNodeId, fromPortIndex, toNodeId, toPortIndex);
            RemoveLinkToNodePort(fromNodeId, fromPortIndex, toNodeId, toPortIndex);
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

        public void Clear() {
            _addedNodesTotalCount = 0;

            _nodesMap.Clear();
            _fromNodePortLinksMap.Clear();
            _toNodePortLinksMap.Clear();
        }

        public bool InvalidateNodePorts(int nodeId, bool invalidateLinks, bool notify = true) {
            if (!_nodesMap.TryGetValue(nodeId, out var nodeMeta)) return false;

            var oldPorts = nodeMeta.Ports;
            int oldPortsCount = oldPorts.Length;

            nodeMeta.RecreatePorts();
            var newPorts = nodeMeta.Ports;
            int newPortsCount = newPorts.Length;

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

                if (invalidateLinks) {
                    if (newPortIndex >= 0) {
                        SetLinksFromNodePort(nodeId, newPortIndex, GetLinksFromNodePort(nodeId, oldPortIndex));
                        SetLinksToNodePort(nodeId, newPortIndex, GetLinksToNodePort(nodeId, oldPortIndex));
                    }

                    RemoveLinksFromNodePort(nodeId, oldPortIndex);
                    RemoveLinksToNodePort(nodeId, oldPortIndex);
                }

                portsChanged = true;
            }

            if (portsChanged && notify) OnInvalidateNodePortsAndLinks?.Invoke(nodeId);

            return portsChanged;
        }

        public void SetSubgraphReference(int nodeId, BlueprintAsset subgraphAsset) {
            _subgraphReferencesMap[nodeId] = subgraphAsset;
        }

        public void RemoveSubgraphReference(int nodeId) {
            if (!_subgraphReferencesMap.ContainsKey(nodeId)) return;

            _subgraphReferencesMap.Remove(nodeId);
        }

        private void CreateConnection(int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
            AddLinkFromNodePort(fromNodeId, fromPortIndex, toNodeId, toPortIndex);
            AddLinkToNodePort(fromNodeId, fromPortIndex, toNodeId, toPortIndex);
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

        private void SetLinksFromNodePort(int nodeId, int portIndex, IEnumerable<BlueprintLink> links) {
            if (!_fromNodePortLinksMap.TryGetValue(nodeId, out var fromNodePortLinksMap)) {
                fromNodePortLinksMap = new SerializedDictionary<int, List<BlueprintLink>>();
                _fromNodePortLinksMap[nodeId] = fromNodePortLinksMap;
            }

            fromNodePortLinksMap[portIndex] = new List<BlueprintLink>(links);
        }

        private void SetLinksToNodePort(int nodeId, int portIndex, IEnumerable<BlueprintLink> links) {
            if (!_toNodePortLinksMap.TryGetValue(nodeId, out var toNodePortLinksMap)) {
                toNodePortLinksMap = new SerializedDictionary<int, List<BlueprintLink>>();
                _toNodePortLinksMap[nodeId] = toNodePortLinksMap;
            }

            toNodePortLinksMap[portIndex] = new List<BlueprintLink>(links);
        }

        private void AddLinkFromNodePort(int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
            if (!_fromNodePortLinksMap.TryGetValue(fromNodeId, out var fromNodePortLinksMap)) {
                fromNodePortLinksMap = new SerializedDictionary<int, List<BlueprintLink>>();
                _fromNodePortLinksMap[fromNodeId] = fromNodePortLinksMap;
            }

            if (!fromNodePortLinksMap.TryGetValue(fromPortIndex, out var fromNodePortLinks)) {
                fromNodePortLinks = new List<BlueprintLink>();
                fromNodePortLinksMap[fromPortIndex] = fromNodePortLinks;
            }

            var link = new BlueprintLink { nodeId = toNodeId, portIndex = toPortIndex };
            fromNodePortLinks.Add(link);
        }

        private void AddLinkToNodePort(int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
            if (!_toNodePortLinksMap.TryGetValue(toNodeId, out var toNodePortLinksMap)) {
                toNodePortLinksMap = new SerializedDictionary<int, List<BlueprintLink>>();
                _toNodePortLinksMap[toNodeId] = toNodePortLinksMap;
            }

            if (!toNodePortLinksMap.TryGetValue(toPortIndex, out var toNodePortLinks)) {
                toNodePortLinks = new List<BlueprintLink>();
                toNodePortLinksMap[toPortIndex] = toNodePortLinks;
            }

            var link = new BlueprintLink { nodeId = fromNodeId, portIndex = fromPortIndex };
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

        public override string ToString() {
            var nodesSb = new StringBuilder();

            foreach ((int nodeId, var nodeMeta) in _nodesMap) {
                nodesSb.AppendLine($"- {nodeMeta}");

                var ports = nodeMeta.Ports;
                for (int p = 0; p < ports.Length; p++) {
                    var portLinks = GetLinksFromNodePort(nodeId, p);
                    nodesSb.AppendLine($"-- port#{p} links: [{string.Join(", ", portLinks)}]");
                }
            }

            return $"{nameof(BlueprintMeta)}(nodes = [\n{nodesSb}\n])";
        }
    }

}
