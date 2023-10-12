using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public sealed class BlueprintMeta2 : IBlueprintMeta {

        [SerializeField] private ArrayMap<long, BlueprintNodeMeta2> _nodeMetaMap;
        [SerializeField] private BlueprintFactorySource _factorySource;
        [SerializeField] private BlueprintLinkStorage _linkStorage;
        [SerializeField] private BlueprintPortStorage _portStorage;

        public IReadOnlyCollection<long> Nodes => _nodeMetaMap.Keys;
        public Action<long> OnInvalidateNode;

        public BlueprintNodeMeta2 GetNode(long id) {
            return _nodeMetaMap[id];
        }

        public long AddNode(Type factoryType, Vector2 position) {
            int factoryId = _factorySource.GetOrCreateFactory(factoryType);
            var factory = _factorySource.GetFactory(factoryId);
            int nodeId = factory.AddNode();

            long id = BlueprintNodeAddress.Pack(factoryId, nodeId);

            _nodeMetaMap[id] = new BlueprintNodeMeta2 {
                nodeId = id,
                position = position
            };

            factory.CreatePorts(this, id);

            return id;
        }

        public void RemoveNode(long id) {
            if (!_nodeMetaMap.ContainsKey(id)) return;

            _linkStorage.RemoveNode(id);
            _nodeMetaMap.Remove(id);
        }

        public void Clear() {
            OnInvalidateNode = null;

            _nodeMetaMap.Clear();
            _factorySource.Clear();
            _linkStorage.Clear();
            _portStorage.Clear();
        }

        public bool TryGetLinksFrom(long id, int port, out int firstLink) {
            return _linkStorage.TryGetLinksFrom(id, port, out firstLink);
        }

        public bool TryGetLinksTo(long id, int port, out int firstLink) {
            return _linkStorage.TryGetLinksTo(id, port, out firstLink);
        }

        public bool TryGetNextLink(int previousLink, out int nextLink) {
            return _linkStorage.TryGetNextLink(previousLink, out nextLink);
        }

        public void AddPort(long id, int index, Port port) {
            _portStorage.AddPort(id, index, port);
        }

        public Port GetLinkedPort(int link) {
            ref var l = ref _linkStorage.GetLink(link);
            return _portStorage.TryGetPort(l.nodeId, l.port, out int port)
                ? _portStorage.GetPortData(port)
                : default;
        }

        public void InvalidateNode(long id, bool invalidatePorts = false) {

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
#endif
*/
    }

}
