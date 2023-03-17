using System;
using System.Collections.Generic;
using MisterGames.Blueprints.Validation;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Meta {

    [Serializable]
    public sealed class BlueprintMeta : IComparer<BlueprintLink> {

        [SerializeField] private SerializedDictionary<int, BlueprintNodeMeta> _nodesMap;
        [SerializeField] private SerializedDictionary<int, SerializedDictionary<int, List<BlueprintLink>>> _fromNodePortLinksMap;

        [SerializeField] private int _addedNodesTotalCount;
        [SerializeField] private SerializedDictionary<int, SerializedDictionary<int, List<BlueprintLink>>> _toNodePortLinksMap;
        [SerializeField] private SerializedDictionary<int, BlueprintAsset> _subgraphReferencesMap;

        public Dictionary<int, BlueprintNodeMeta> NodesMap => _nodesMap;

        public IReadOnlyList<BlueprintLink> GetLinksFromNodePort(int nodeId, int portIndex) {
            if (!_fromNodePortLinksMap.TryGetValue(nodeId, out var fromNodePortLinksMap) ||
                !fromNodePortLinksMap.TryGetValue(portIndex, out var fromNodePortLinks)
            ) {
                return Array.Empty<BlueprintLink>();
            }

            fromNodePortLinks.Sort(this);

            return fromNodePortLinks;
        }

        int IComparer<BlueprintLink>.Compare(BlueprintLink x, BlueprintLink y) {
            return _nodesMap[x.nodeId].Position.y.CompareTo(_nodesMap[y.nodeId].Position.y);
        }

#if UNITY_EDITOR
        public Action<int> OnInvalidateNodePortsAndLinks;
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

            var fromPort = fromNode.Ports[fromPortIndex];
            var toPort = toNode.Ports[toPortIndex];

            if (!PortValidator.ArePortsCompatible(fromPort, toPort)) return false;

            // FromPort is port that owns links to toPort (fromPort must be exit or data-based input port).
            // So ports have to be swapped, if:
            // 1) fromPort is enter port (IsData = false, IsInput = true)
            // 2) fromPort is data-based output port (IsData = true, IsInput = false)
            if (fromPort.IsData != fromPort.IsInput) (fromNodeId, fromPort, fromPortIndex) = (toNodeId, toPort, toPortIndex);

            if (HasLinkFromNodePort(fromNodeId, fromPortIndex, toNodeId, toPortIndex)) return false;
            if (HasLinkToNodePort(fromNodeId, fromPortIndex, toNodeId, toPortIndex)) return false;

            if (!fromPort.IsMultiple &&
                _fromNodePortLinksMap.TryGetValue(fromNodeId, out var fromNodePortLinksMap) &&
                fromNodePortLinksMap != null && fromNodePortLinksMap.TryGetValue(fromPortIndex, out var fromPortLinks) &&
                fromPortLinks is { Count: > 0 }
            ) {
                RemoveAllLinksFromNodePort(fromNodeId, fromPortIndex);
            }

            if (!toPort.IsMultiple &&
                _toNodePortLinksMap.TryGetValue(toNodeId, out var toNodePortLinksMap) &&
                toNodePortLinksMap != null && toNodePortLinksMap.TryGetValue(toPortIndex, out var toPortLinks) &&
                toPortLinks is { Count: > 0 }
            ) {
                RemoveAllLinksToNodePort(toNodeId, toPortIndex);
            }

            AddLinkFromNodePort(fromNodeId, fromPortIndex, toNodeId, toPortIndex);
            AddLinkToNodePort(fromNodeId, fromPortIndex, toNodeId, toPortIndex);
            return true;
        }

        public void RemoveConnection(int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
            RemoveLinkFromNodePort(fromNodeId, fromPortIndex, toNodeId, toPortIndex);
            RemoveLinkFromNodePort(toNodeId, toPortIndex, fromNodeId, fromPortIndex);
            RemoveLinkToNodePort(fromNodeId, fromPortIndex, toNodeId, toPortIndex);
            RemoveLinkToNodePort(toNodeId, toPortIndex, fromNodeId, fromPortIndex);
        }

        public void Clear() {
            _addedNodesTotalCount = 0;

            _nodesMap.Clear();
            _fromNodePortLinksMap.Clear();
            _toNodePortLinksMap.Clear();
            _subgraphReferencesMap.Clear();
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
                var oldPort = oldPorts[oldPortIndex];
                int newPortIndex = -1;

                // Try find port with same signature in range from old port index to the last port in new ports.
                for (int np = oldPortIndex; np < newPortsCount; np++) {
                    if (oldPort != newPorts[np]) continue;

                    newPortIndex = np;
                    break;
                }

                // Port with same signature is found at same index as an old port, old links are valid.
                if (newPortIndex == oldPortIndex) continue;

                portsChanged = true;

                if (!invalidateLinks) continue;

                // If new port index is not found at first attempt,
                // try find port with same signature in range from 0 to old port index - 1
                // or to the last port in new ports if old port index exceeds new ports range.
                if (newPortIndex < 0) {
                    int count = Math.Min(oldPortIndex, newPortsCount);
                    for (int np = 0; np < count; np++) {
                        if (oldPort != newPorts[np]) continue;

                        newPortIndex = np;
                        break;
                    }
                }

                // Found port with same signature on new index, adding old links at new port index
                if (newPortIndex >= 0) {
                    if (!hasInstantiatedPortLinksMapsCache) {
                        if (_fromNodePortLinksMap.TryGetValue(nodeId, out var fromNodePortLinksMap)) {
                            fromPortLinksMapCache = new Dictionary<int, List<BlueprintLink>>(fromNodePortLinksMap);
                        }

                        if (_toNodePortLinksMap.TryGetValue(nodeId, out var toNodePortLinksMap)) {
                            toPortLinksMapCache = new Dictionary<int, List<BlueprintLink>>(toNodePortLinksMap);
                        }

                        hasInstantiatedPortLinksMapsCache = true;
                    }

                    var fromPortLinks =
                        fromPortLinksMapCache != null &&
                        fromPortLinksMapCache.TryGetValue(oldPortIndex, out var fromPortLinksCache)
                            ? fromPortLinksCache
                            : (IReadOnlyList<BlueprintLink>) Array.Empty<BlueprintLink>();

                    var toPortLinks =
                        toPortLinksMapCache != null &&
                        toPortLinksMapCache.TryGetValue(oldPortIndex, out var toPortLinksCache)
                            ? toPortLinksCache
                            : (IReadOnlyList<BlueprintLink>) Array.Empty<BlueprintLink>();

                    SetLinksFromNodePort(nodeId, newPortIndex, fromPortLinks);
                    SetLinksToNodePort(nodeId, newPortIndex, toPortLinks);
                }

                RemoveAllLinksFromNodePort(nodeId, oldPortIndex);
                RemoveAllLinksToNodePort(nodeId, oldPortIndex);
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
#endif
    }

}
