using System;
using System.Collections.Generic;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Validation;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public sealed class BlueprintNodeMetaStorage : IComparer<BlueprintLink2> {

        [SerializeField] private ArrayMap<long, BlueprintNodeMeta2> _nodeMetaMap;
        [SerializeField] private BlueprintLinkStorage _fromPortLinks;
        [SerializeField] private BlueprintLinkStorage _toPortLinks;

        public Action<long> OnInvalidateNodePortsAndLinks;

        public ref BlueprintNodeMeta2 GetBlueprintNodeMeta(long id) {
            if (!_nodeMetaMap.ContainsKey(id)) {
                // log
            }

            return ref _nodeMetaMap.GetValueByRef(id);
        }

        public long AddBlueprintNodeMeta(BlueprintNodeMeta2 nodeMeta) {
            long id = BlueprintNodeAddress.Pack(nodeMeta.factoryId, nodeMeta.nodeId);

            if (_nodeMetaMap.ContainsKey(id)) {
                // log
                return 0;
            }

            _nodeMetaMap.Add(id, nodeMeta);

            return id;
        }
/*
        public void RemoveBlueprintNodeMeta(long id) {
            if (!_nodeMetaMap.ContainsKey(id)) {
                // log
                return;
            }

            BlueprintNodeAddress.Unpack(id, out int factoryId, out int nodeId);

            _fromPortLinks.RemoveNodeLinks(factoryId, nodeId);
            _toPortLinks.RemoveNodeLinks(factoryId, nodeId);

            _nodeMetaMap.Remove(id);
        }

        public IReadOnlyList<BlueprintLink2> GetLinksFromNodePort(int nodeId, int portIndex) {
            if (!_fromNodePortLinksMap.TryGetValue(nodeId, out var fromNodePortLinksMap) ||
                !fromNodePortLinksMap.TryGetValue(portIndex, out var fromNodePortLinks)
            ) {
                return Array.Empty<BlueprintRuntimeLink>();
            }

            fromNodePortLinks.Sort(this);

            return fromNodePortLinks;
        }
*/
        int IComparer<BlueprintLink2>.Compare(BlueprintLink2 x, BlueprintLink2 y) {
            return 0;//_nodesMap[x.nodeId].Position.y.CompareTo(_nodesMap[y.nodeId].Position.y);
        }
/*
#if UNITY_EDITOR




        public bool TryCreateConnection(long fromNodeId, int fromPortIndex, long toNodeId, int toPortIndex) {
            if (fromNodeId == toNodeId) return false;

            if (!_nodeMetaMap.ContainsKey(fromNodeId)) return false;
            if (!_nodeMetaMap.ContainsKey(toNodeId)) return false;

            ref var fromNodeMeta = ref _nodeMetaMap.Get(fromNodeId);
            ref var toNodeMeta = ref _nodeMetaMap.Get(toNodeId);

            if (fromPortIndex < 0 || fromPortIndex > fromNodeMeta.ports.Length - 1) return false;
            if (toPortIndex < 0 || toPortIndex > toNodeMeta.ports.Length - 1) return false;

            ref var fromPort = ref fromNodeMeta.ports[fromPortIndex];
            ref var toPort = ref toNodeMeta.ports[toPortIndex];

            if (!PortValidator.ArePortsCompatible(fromPort, toPort)) return false;

            // FromPort is port that owns links to toPort (fromPort must be exit or data-based input port).
            // So ports have to be swapped, if:
            // 1) fromPort is enter port (IsData = false, IsInput = true)
            // 2) fromPort is data-based output port (IsData = true, IsInput = false)
            if (fromPort.IsData != fromPort.IsInput) {
                long t0 = fromNodeId;
                int t1 = fromPortIndex;
                var t2 = fromPort;

                fromNodeId = toNodeId;
                fromPortIndex = toPortIndex;
                fromPort = toPort;

                toNodeId = t0;
                toPortIndex = t1;
                toPort = t2;
            }

            if (_fromPortLinks.HasLink(fromNodeId, fromPortIndex, toNodeId, toPortIndex)) return false;
            if (_toPortLinks.HasLink(toNodeId, toPortIndex, fromNodeId, fromPortIndex)) return false;

            _fromPortLinks.GetLinks(fromNodeId, fromPortIndex, out int fromPortLinksIndex, out int fromPortLinksCount);
            _toPortLinks.GetLinks(toNodeId, toPortIndex, out int toPortLinksIndex, out int toPortLinksCount);

            if (!fromPort.IsMultiple && fromPortLinksCount > 0) {
                int end = fromPortLinksIndex + fromPortLinksCount;
                for (int i = fromPortLinksIndex; i < end; i++) {
                    var l = _fromPortLinks.GetLink(i);
                    _toPortLinks.RemoveLink(l.nodeId, l.port, fromNodeId, fromPortIndex);
                }
                _fromPortLinks.RemovePortLinks(fromNodeId, fromPortIndex);
            }

            if (!toPort.IsMultiple && toPortLinksCount > 0) {
                int end = toPortLinksIndex + toPortLinksCount;
                for (int i = toPortLinksIndex; i < end; i++) {
                    var l = _toPortLinks.GetLink(i);
                    _fromPortLinks.RemoveLink(l.nodeId, l.port, toNodeId, toPortIndex);
                }
                _toPortLinks.RemovePortLinks(toNodeId, toPortIndex);
            }

            _fromPortLinks.AddLink(fromNodeId, fromPortIndex, toNodeId, toPortIndex);
            _toPortLinks.AddLink(toNodeId, toPortIndex, fromNodeId, fromPortIndex);

            return true;
        }

        public void RemoveConnection(BlueprintAsset blueprint, int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
            RemoveLinkFromNodePort(blueprint, fromNodeId, fromPortIndex, toNodeId, toPortIndex);
            RemoveLinkFromNodePort(blueprint, toNodeId, toPortIndex, fromNodeId, fromPortIndex);
            RemoveLinkToNodePort(blueprint, fromNodeId, fromPortIndex, toNodeId, toPortIndex);
            RemoveLinkToNodePort(blueprint, toNodeId, toPortIndex, fromNodeId, fromPortIndex);
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

        public bool InvalidateNodePorts(BlueprintAsset blueprint, int nodeId, bool invalidateLinks, bool notify = true) {
            if (!_nodesMap.TryGetValue(nodeId, out var nodeMeta)) return false;

            var oldPorts = nodeMeta.Ports;
            int oldPortsCount = oldPorts.Length;

            nodeMeta.RecreatePorts(blueprint);
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

                RemoveAllLinksFromNodePort(blueprint, nodeId, oldPortIndex);
                RemoveAllLinksToNodePort(blueprint, nodeId, oldPortIndex);
            }

            if (portsChanged && notify) OnInvalidateNodePortsAndLinks?.Invoke(nodeId);

            return portsChanged;
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

        private void AddLinkFromNodePort(BlueprintAsset blueprint, int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
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
                fromNodeListener.OnPortLinksChanged(blueprint, fromNodeId, fromPortIndex);
            }
        }

        private void AddLinkToNodePort(BlueprintAsset blueprint, int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
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
                toNodeListener.OnPortLinksChanged(blueprint, toNodeId, toPortIndex);
            }
        }

        private void RemoveLinkFromNodePort(BlueprintAsset blueprint, int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
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
                fromNodeListener.OnPortLinksChanged(blueprint, fromNodeId, fromPortIndex);
            }

            if (portLinks.Count == 0) portLinksMap.Remove(fromPortIndex);
            if (portLinksMap.Count == 0) _fromNodePortLinksMap.Remove(fromNodeId);
        }

        private void RemoveLinkToNodePort(BlueprintAsset blueprint, int fromNodeId, int fromPortIndex, int toNodeId, int toPortIndex) {
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
                toNodeListener.OnPortLinksChanged(blueprint, toNodeId, toPortIndex);
            }

            if (portLinks.Count == 0) portLinksMap.Remove(toPortIndex);
            if (portLinksMap.Count == 0) _toNodePortLinksMap.Remove(toNodeId);
        }

        private void RemoveAllLinksFromNodePort(BlueprintAsset blueprint, int nodeId, int portIndex) {
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
                    fromNodeListener.OnPortLinksChanged(blueprint, nodeId, portIndex);
                }

                if (linkedPortLinks.Count == 0) linkedNodePortLinksMap.Remove(link.portIndex);
                if (linkedNodePortLinksMap.Count == 0) _toNodePortLinksMap.Remove(link.nodeId);
            }

            portLinksMap.Remove(portIndex);
            if (portLinksMap.Count == 0) _fromNodePortLinksMap.Remove(nodeId);
        }

        private void RemoveAllLinksToNodePort(BlueprintAsset blueprint, int nodeId, int portIndex) {
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
                    toNodeListener.OnPortLinksChanged(blueprint, nodeId, portIndex);
                }

                if (linkedPortLinks.Count == 0) linkedNodePortLinksMap.Remove(link.portIndex);
                if (linkedNodePortLinksMap.Count == 0) _fromNodePortLinksMap.Remove(link.nodeId);
            }

            portLinksMap.Remove(portIndex);
            if (portLinksMap.Count == 0) _toNodePortLinksMap.Remove(nodeId);
        }

        private void RemoveLinksFromNode(BlueprintAsset blueprint, int nodeId) {
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
                        fromNodeListener.OnPortLinksChanged(blueprint, nodeId, portIndex);
                    }

                    if (linkedPortLinks.Count == 0) linkedNodePortLinksMap.Remove(link.portIndex);
                    if (linkedNodePortLinksMap.Count == 0) _toNodePortLinksMap.Remove(link.nodeId);
                }
            }

            _fromNodePortLinksMap.Remove(nodeId);
        }

        private void RemoveLinksToNode(BlueprintAsset blueprint, int nodeId) {
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
                        toNodeListener.OnPortLinksChanged(blueprint, nodeId, portIndex);
                    }

                    if (linkedPortLinks.Count == 0) linkedNodePortLinksMap.Remove(link.portIndex);
                    if (linkedNodePortLinksMap.Count == 0) _fromNodePortLinksMap.Remove(link.nodeId);
                }
            }

            _toNodePortLinksMap.Remove(nodeId);
        }
#endif
*/
    }

}
