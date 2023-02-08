﻿using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Meta {

    [Serializable]
    public sealed class BlueprintMeta : IComparer<BlueprintLink> {

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

            if (fromPort.mode is Port.Mode.Input) {
                if (toPort.mode is not (Port.Mode.Output or Port.Mode.NonTypedOutput)) return false;

                // input and output must have same data type
                if (toPort.mode == Port.Mode.Output && fromPort.DataType != toPort.DataType) return false;

                // replacing connections from the input port fromPort to the output port toPort with new connection
                RemoveAllLinksFromNodePort(fromNodeId, fromPortIndex);
                CreateConnection(fromNodeId, fromPortIndex, toNodeId, toPortIndex);
                return true;
            }

            if (fromPort.mode is Port.Mode.InputArray) {
                if (toPort.mode is not (Port.Mode.Output or Port.Mode.NonTypedOutput)) return false;

                // input and output must have same data type
                if (toPort.mode == Port.Mode.Output && fromPort.DataType != toPort.DataType) return false;

                // add connection from the input port fromPort to the output port toPort
                CreateConnection(fromNodeId, fromPortIndex, toNodeId, toPortIndex);
                return true;
            }

            if (fromPort.mode is Port.Mode.NonTypedInput) {
                if (toPort.mode is not Port.Mode.Output) return false;

                // replacing connections from the input port fromPort to the output port toPort with new connection
                RemoveAllLinksFromNodePort(fromNodeId, fromPortIndex);
                CreateConnection(fromNodeId, fromPortIndex, toNodeId, toPortIndex);
                return true;
            }

            if (fromPort.mode is Port.Mode.Output) {
                if (toPort.mode is not (Port.Mode.Input or Port.Mode.InputArray or Port.Mode.NonTypedInput)) return false;

                // input and output must have same data type
                if (toPort.mode is (Port.Mode.Input or Port.Mode.InputArray) && fromPort.DataType != toPort.DataType) return false;

                // replacing connections from the input port toPort to the output port fromPort with new connection
                if (toPort.mode is Port.Mode.Input or Port.Mode.NonTypedInput) RemoveAllLinksFromNodePort(toNodeId, toPortIndex);

                // adding connection from the input port toPort to the output port fromPort
                CreateConnection(toNodeId, toPortIndex, fromNodeId, fromPortIndex);
                return true;
            }

            if (fromPort.mode is Port.Mode.NonTypedOutput) {
                if (toPort.mode is not (Port.Mode.Input or Port.Mode.InputArray)) return false;

                // replacing connections from the input port toPort to the output port fromPort with new connection
                if (toPort.mode is Port.Mode.Input) RemoveAllLinksFromNodePort(toNodeId, toPortIndex);

                // adding connection from the input port toPort to the output port fromPort
                CreateConnection(toNodeId, toPortIndex, fromNodeId, fromPortIndex);
                return true;
            }

            return false;
        }

        public void RemoveConnection(int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
            RemoveLinkFromNodePort(fromNodeId, fromPortIndex, toNodeId, toPortIndex);
            RemoveLinkFromNodePort(toNodeId, toPortIndex, fromNodeId, fromPortIndex);
            RemoveLinkToNodePort(fromNodeId, fromPortIndex, toNodeId, toPortIndex);
            RemoveLinkToNodePort(toNodeId, toPortIndex, fromNodeId, fromPortIndex);
        }

        public IReadOnlyList<BlueprintLink> GetLinksFromNodePort(int nodeId, int portIndex) {
            if (!_fromNodePortLinksMap.TryGetValue(nodeId, out var fromNodePortLinksMap) ||
                !fromNodePortLinksMap.TryGetValue(portIndex, out var fromNodePortLinks)
            ) {
                return Array.Empty<BlueprintLink>();
            }

            fromNodePortLinks.Sort(this);

            return fromNodePortLinks;
        }

        public IReadOnlyList<BlueprintLink> GetLinksToNodePort(int nodeId, int portIndex) {
            if (!_toNodePortLinksMap.TryGetValue(nodeId, out var toNodePortLinksMap) ||
                !toNodePortLinksMap.TryGetValue(portIndex, out var toNodePortLinks)
            ) {
                return Array.Empty<BlueprintLink>();
            }

            toNodePortLinks.Sort(this);

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

            nodeMeta.RecreatePorts(this);
            var newPorts = nodeMeta.Ports;
            int newPortsCount = newPorts.Length;

            bool portsChanged = oldPortsCount != newPortsCount;

            Dictionary<int, List<BlueprintLink>> fromPortLinksMapCache = null;
            Dictionary<int, List<BlueprintLink>> toPortLinksMapCache = null;
            bool hasInstantiatedPortLinksMapsCache = false;

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
                    if (!hasInstantiatedPortLinksMapsCache) {
                        if (_fromNodePortLinksMap.TryGetValue(nodeId, out var fromNodePortLinksMap)) {
                            fromPortLinksMapCache = new Dictionary<int, List<BlueprintLink>>(fromNodePortLinksMap);
                        }

                        if (_toNodePortLinksMap.TryGetValue(nodeId, out var toNodePortLinksMap)) {
                            toPortLinksMapCache = new Dictionary<int, List<BlueprintLink>>(toNodePortLinksMap);
                        }

                        hasInstantiatedPortLinksMapsCache = true;
                    }

                    if (newPortIndex >= 0) {
                        IReadOnlyList<BlueprintLink> fromPortLinks =
                            fromPortLinksMapCache != null &&
                            fromPortLinksMapCache.TryGetValue(oldPortIndex, out var fromPortLinksCache)
                                ? fromPortLinksCache
                                : Array.Empty<BlueprintLink>();

                        IReadOnlyList<BlueprintLink> toPortLinks =
                            toPortLinksMapCache != null &&
                            toPortLinksMapCache.TryGetValue(oldPortIndex, out var toPortLinksCache)
                                ? toPortLinksCache
                                : Array.Empty<BlueprintLink>();

                        SetLinksFromNodePort(nodeId, newPortIndex, fromPortLinks);
                        SetLinksToNodePort(nodeId, newPortIndex, toPortLinks);
                    }

                    RemoveAllLinksFromNodePort(nodeId, oldPortIndex);
                    RemoveAllLinksToNodePort(nodeId, oldPortIndex);
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

        private void SetLinksFromNodePort(int nodeId, int portIndex, IReadOnlyList<BlueprintLink> links) {
            if (!_fromNodePortLinksMap.TryGetValue(nodeId, out var fromNodePortLinksMap)) {
                fromNodePortLinksMap = new SerializedDictionary<int, List<BlueprintLink>>();
                _fromNodePortLinksMap[nodeId] = fromNodePortLinksMap;
            }

            fromNodePortLinksMap[portIndex] = new List<BlueprintLink>(links);

            for (int l = 0; l < links.Count; l++) {
                var link = links[l];

                if (!_toNodePortLinksMap.TryGetValue(link.nodeId, out var toNodePortLinksMap)) {
                    toNodePortLinksMap = new SerializedDictionary<int, List<BlueprintLink>>();
                    _toNodePortLinksMap[link.nodeId] = toNodePortLinksMap;
                }

                if (!toNodePortLinksMap.TryGetValue(link.portIndex, out var toNodePortLinks)) {
                    toNodePortLinks = new List<BlueprintLink>(1);
                    toNodePortLinksMap[link.portIndex] = toNodePortLinks;
                }

                toNodePortLinks.Add(new BlueprintLink { nodeId = nodeId, portIndex = portIndex });
            }

            if (links.Count == 0) fromNodePortLinksMap.Remove(portIndex);
            if (fromNodePortLinksMap.Count == 0) _fromNodePortLinksMap.Remove(nodeId);
        }

        private void SetLinksToNodePort(int nodeId, int portIndex, IReadOnlyList<BlueprintLink> links) {
            if (!_toNodePortLinksMap.TryGetValue(nodeId, out var toNodePortLinksMap)) {
                toNodePortLinksMap = new SerializedDictionary<int, List<BlueprintLink>>();
                _toNodePortLinksMap[nodeId] = toNodePortLinksMap;
            }

            toNodePortLinksMap[portIndex] = new List<BlueprintLink>(links);

            for (int l = 0; l < links.Count; l++) {
                var link = links[l];

                if (!_fromNodePortLinksMap.TryGetValue(link.nodeId, out var fromNodePortLinksMap)) {
                    fromNodePortLinksMap = new SerializedDictionary<int, List<BlueprintLink>>();
                    _fromNodePortLinksMap[link.nodeId] = fromNodePortLinksMap;
                }

                if (!fromNodePortLinksMap.TryGetValue(link.portIndex, out var fromNodePortLinks)) {
                    fromNodePortLinks = new List<BlueprintLink>(1);
                    fromNodePortLinksMap[link.portIndex] = fromNodePortLinks;
                }

                fromNodePortLinks.Add(new BlueprintLink { nodeId = nodeId, portIndex = portIndex });
            }

            if (links.Count == 0) toNodePortLinksMap.Remove(portIndex);
            if (toNodePortLinksMap.Count == 0) _toNodePortLinksMap.Remove(nodeId);
        }

        private void AddLinkFromNodePort(int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
            if (!_fromNodePortLinksMap.TryGetValue(fromNodeId, out var fromNodePortLinksMap)) {
                fromNodePortLinksMap = new SerializedDictionary<int, List<BlueprintLink>>();
                _fromNodePortLinksMap[fromNodeId] = fromNodePortLinksMap;
            }

            if (!fromNodePortLinksMap.TryGetValue(fromPortIndex, out var fromNodePortLinks)) {
                fromNodePortLinks = new List<BlueprintLink>(1);
                fromNodePortLinksMap[fromPortIndex] = fromNodePortLinks;
            }

            var link = new BlueprintLink { nodeId = toNodeId, portIndex = toPortIndex };
            fromNodePortLinks.Add(link);

            if (_nodesMap.TryGetValue(fromNodeId, out var fromNodeMeta) && fromNodeMeta.Node is IBlueprintPortLinksListener fromNodeListener) {
                fromNodeListener.OnPortLinksChanged(this, fromNodeId, fromPortIndex);
            }
        }

        private void AddLinkToNodePort(int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
            if (!_toNodePortLinksMap.TryGetValue(toNodeId, out var toNodePortLinksMap)) {
                toNodePortLinksMap = new SerializedDictionary<int, List<BlueprintLink>>();
                _toNodePortLinksMap[toNodeId] = toNodePortLinksMap;
            }

            if (!toNodePortLinksMap.TryGetValue(toPortIndex, out var toNodePortLinks)) {
                toNodePortLinks = new List<BlueprintLink>(1);
                toNodePortLinksMap[toPortIndex] = toNodePortLinks;
            }

            var link = new BlueprintLink { nodeId = fromNodeId, portIndex = fromPortIndex };
            toNodePortLinks.Add(link);

            if (_nodesMap.TryGetValue(toNodeId, out var toNodeMeta) && toNodeMeta.Node is IBlueprintPortLinksListener toNodeListener) {
                toNodeListener.OnPortLinksChanged(this, toNodeId, toPortIndex);
            }
        }

        private void RemoveLinkFromNodePort(int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
            if (!_fromNodePortLinksMap.TryGetValue(fromNodeId, out var portLinksMap)) return;
            if (!portLinksMap.TryGetValue(fromPortIndex, out var portLinks)) return;

            bool removed = false;

            for (int i = 0; i < portLinks.Count; i++) {
                var link = portLinks[i];
                if (link.nodeId != toNodeId || link.portIndex != toPortIndex) continue;

                portLinks.RemoveAt(i);
                removed = true;
            }

            if (removed && _nodesMap.TryGetValue(fromNodeId, out var fromNodeMeta) && fromNodeMeta.Node is IBlueprintPortLinksListener fromNodeListener) {
                fromNodeListener.OnPortLinksChanged(this, fromNodeId, fromPortIndex);
            }

            if (portLinks.Count == 0) portLinksMap.Remove(fromPortIndex);
            if (portLinksMap.Count == 0) _fromNodePortLinksMap.Remove(fromNodeId);
        }

        private void RemoveLinkToNodePort(int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
            if (!_toNodePortLinksMap.TryGetValue(toNodeId, out var portLinksMap)) return;
            if (!portLinksMap.TryGetValue(toPortIndex, out var portLinks)) return;

            bool removed = false;

            for (int i = 0; i < portLinks.Count; i++) {
                var link = portLinks[i];
                if (link.nodeId != fromNodeId || link.portIndex != fromPortIndex) continue;

                portLinks.RemoveAt(i);
                removed = true;
            }

            if (removed && _nodesMap.TryGetValue(toNodeId, out var toNodeMeta) && toNodeMeta.Node is IBlueprintPortLinksListener toNodeListener) {
                toNodeListener.OnPortLinksChanged(this, toNodeId, toPortIndex);
            }

            if (portLinks.Count == 0) portLinksMap.Remove(toPortIndex);
            if (portLinksMap.Count == 0) _toNodePortLinksMap.Remove(toNodeId);
        }

        private void RemoveAllLinksFromNodePort(int nodeId, int portIndex) {
            if (!_fromNodePortLinksMap.TryGetValue(nodeId, out var portLinksMap)) return;
            if (!portLinksMap.TryGetValue(portIndex, out var links)) return;

            for (int i = 0; i < links.Count; i++) {
                var link = links[i];

                if (!_toNodePortLinksMap.TryGetValue(link.nodeId, out var linkedNodePortLinksMap)) continue;
                if (!linkedNodePortLinksMap.TryGetValue(link.portIndex, out var linkedPortLinks)) continue;

                bool removed = false;

                for (int j = 0; j < linkedPortLinks.Count; j++) {
                    var linkedPortLink = linkedPortLinks[j];
                    if (linkedPortLink.nodeId != nodeId || linkedPortLink.portIndex != portIndex) continue;

                    linkedPortLinks.RemoveAt(j);
                    removed = true;
                }

                if (removed && _nodesMap.TryGetValue(nodeId, out var fromNodeMeta) && fromNodeMeta.Node is IBlueprintPortLinksListener fromNodeListener) {
                    fromNodeListener.OnPortLinksChanged(this, nodeId, portIndex);
                }

                if (linkedPortLinks.Count == 0) linkedNodePortLinksMap.Remove(link.portIndex);
                if (linkedNodePortLinksMap.Count == 0) _toNodePortLinksMap.Remove(link.nodeId);
            }

            portLinksMap.Remove(portIndex);
            if (portLinksMap.Count == 0) _fromNodePortLinksMap.Remove(nodeId);
        }

        private void RemoveAllLinksToNodePort(int nodeId, int portIndex) {
            if (!_toNodePortLinksMap.TryGetValue(nodeId, out var portLinksMap)) return;
            if (!portLinksMap.TryGetValue(portIndex, out var links)) return;

            for (int i = 0; i < links.Count; i++) {
                var link = links[i];

                if (!_fromNodePortLinksMap.TryGetValue(link.nodeId, out var linkedNodePortLinksMap)) continue;
                if (!linkedNodePortLinksMap.TryGetValue(link.portIndex, out var linkedPortLinks)) continue;

                bool removed = false;

                for (int j = 0; j < linkedPortLinks.Count; j++) {
                    var linkedPortLink = linkedPortLinks[j];
                    if (linkedPortLink.nodeId != nodeId || linkedPortLink.portIndex != portIndex) continue;

                    linkedPortLinks.RemoveAt(j);
                    removed = true;
                }

                if (removed && _nodesMap.TryGetValue(nodeId, out var toNodeMeta) && toNodeMeta.Node is IBlueprintPortLinksListener toNodeListener) {
                    toNodeListener.OnPortLinksChanged(this, nodeId, portIndex);
                }

                if (linkedPortLinks.Count == 0) linkedNodePortLinksMap.Remove(link.portIndex);
                if (linkedNodePortLinksMap.Count == 0) _fromNodePortLinksMap.Remove(link.nodeId);
            }

            portLinksMap.Remove(portIndex);
            if (portLinksMap.Count == 0) _toNodePortLinksMap.Remove(nodeId);
        }

        private void RemoveLinksFromNode(int nodeId) {
            if (!_fromNodePortLinksMap.TryGetValue(nodeId, out var portLinksMap)) return;

            foreach ((int portIndex, var links) in portLinksMap) {
                for (int i = 0; i < links.Count; i++) {
                    var link = links[i];

                    if (!_toNodePortLinksMap.TryGetValue(link.nodeId, out var linkedNodePortLinksMap)) continue;
                    if (!linkedNodePortLinksMap.TryGetValue(link.portIndex, out var linkedPortLinks)) continue;

                    bool removed = false;

                    for (int j = 0; j < linkedPortLinks.Count; j++) {
                        var linkedPortLink = linkedPortLinks[j];
                        if (linkedPortLink.nodeId != nodeId) continue;

                        linkedPortLinks.RemoveAt(j);
                        removed = true;
                    }

                    if (removed && _nodesMap.TryGetValue(nodeId, out var fromNodeMeta) && fromNodeMeta.Node is IBlueprintPortLinksListener fromNodeListener) {
                        fromNodeListener.OnPortLinksChanged(this, nodeId, portIndex);
                    }

                    if (linkedPortLinks.Count == 0) linkedNodePortLinksMap.Remove(link.portIndex);
                    if (linkedNodePortLinksMap.Count == 0) _toNodePortLinksMap.Remove(link.nodeId);
                }
            }

            _fromNodePortLinksMap.Remove(nodeId);
        }

        private void RemoveLinksToNode(int nodeId) {
            if (!_toNodePortLinksMap.TryGetValue(nodeId, out var portLinksMap)) return;

            foreach ((int portIndex, var links) in portLinksMap) {
                for (int i = 0; i < links.Count; i++) {
                    var link = links[i];

                    if (!_fromNodePortLinksMap.TryGetValue(link.nodeId, out var linkedNodePortLinksMap)) continue;
                    if (!linkedNodePortLinksMap.TryGetValue(link.portIndex, out var linkedPortLinks)) continue;

                    bool removed = false;

                    for (int j = 0; j < linkedPortLinks.Count; j++) {
                        var linkedPortLink = linkedPortLinks[j];
                        if (linkedPortLink.nodeId != nodeId) continue;

                        linkedPortLinks.RemoveAt(j);
                        removed = true;
                    }

                    if (removed && _nodesMap.TryGetValue(nodeId, out var toNodeMeta) && toNodeMeta.Node is IBlueprintPortLinksListener toNodeListener) {
                        toNodeListener.OnPortLinksChanged(this, nodeId, portIndex);
                    }

                    if (linkedPortLinks.Count == 0) linkedNodePortLinksMap.Remove(link.portIndex);
                    if (linkedNodePortLinksMap.Count == 0) _fromNodePortLinksMap.Remove(link.nodeId);
                }
            }

            _toNodePortLinksMap.Remove(nodeId);
        }

        int IComparer<BlueprintLink>.Compare(BlueprintLink x, BlueprintLink y) {
            return _nodesMap[x.nodeId].Position.y.CompareTo(_nodesMap[y.nodeId].Position.y);
        }
    }

}
