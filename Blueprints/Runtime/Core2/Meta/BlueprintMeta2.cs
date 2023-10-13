using System;
using System.Collections.Generic;
using MisterGames.Blueprints.Validation;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public sealed class BlueprintMeta2 : IBlueprintMeta {

        [SerializeField] private ArrayMap<long, BlueprintNodeMeta2> _nodeMap;
        [SerializeField] private SerializedDictionary<long, BlueprintAsset2> _subgraphMap;
        [SerializeField] private BlueprintFactory _factory;
        [SerializeField] private BlueprintLinkStorage _links;
        [SerializeField] private BlueprintPortStorage _ports;

        public IReadOnlyCollection<long> Nodes => _nodeMap.Keys;

        private Action<long> _onNodeChanged;

        public void Bind(Action<long> onNodeChanged) {
            _onNodeChanged = onNodeChanged;
            _links.OnPortChanged = OnPortChanged;
        }

        public void Unbind() {
            _onNodeChanged = null;
            _links.OnPortChanged = null;
        }

        public BlueprintNodeMeta2 GetNode(long id) {
            return _nodeMap[id];
        }

        public string GetNodePath(long id) {
            BlueprintNodeAddress.Unpack(id, out int sourceId, out int nodeId);
            return $"{nameof(_factory)}.{_factory.GetSourcePath(sourceId)}.{_factory.GetSource(sourceId).GetNodePath(nodeId)}";
        }

        public long AddNode(Type sourceType, Vector2 position) {
            int sourceId = _factory.GetOrCreateSource(sourceType);
            var source = _factory.GetSource(sourceId);
            int nodeId = source.AddNode();

            long id = BlueprintNodeAddress.Pack(sourceId, nodeId);
            _nodeMap[id] = new BlueprintNodeMeta2 { nodeId = id, position = position };

            source.CreatePorts(this, id);

            _onNodeChanged?.Invoke(id);

            return id;
        }

        public void RemoveNode(long id) {
            if (!_nodeMap.ContainsKey(id)) return;

            _nodeMap.Remove(id);
            _subgraphMap.Remove(id);
            _links.RemoveNode(id);
            _ports.RemoveNode(id);

            BlueprintNodeAddress.Unpack(id, out int sourceId, out int nodeId);

            var source = _factory.GetSource(sourceId);

            source.RemoveNode(nodeId);
            if (source.Count == 0) _factory.RemoveSource(sourceId);
        }

        public bool TryCreateConnection(long id, int port, long toId, int toPort) {
            if (id == toId) return false;

            if (!_nodeMap.ContainsKey(id)) return false;
            if (!_nodeMap.ContainsKey(toId)) return false;

            if (!_ports.TryGetPort(id, port, out int portIndex)) return false;
            if (!_ports.TryGetPort(toId, toPort, out int toPortIndex)) return false;

            var portData = _ports.GetPortData(portIndex);
            var toPortData = _ports.GetPortData(toPortIndex);

            if (!PortValidator.ArePortsCompatible(portData, toPortData)) return false;

            // FromPort is port that owns links to toPort (fromPort must be exit or data-based input port).
            // So ports have to be swapped, if:
            // 1) fromPort is enter port (IsData = false, IsInput = true)
            // 2) fromPort is data-based output port (IsData = true, IsInput = false)
            if (portData.IsData() != portData.IsInput()) {
                long t0 = id;
                int t1 = port;
                var t2 = portData;

                id = toId;
                port = toPort;
                portData = toPortData;

                toId = t0;
                toPort = t1;
                toPortData = t2;
            }

            if (_links.ContainsLink(id, port, toId, toPort)) return false;

            if (!portData.IsMultiple() && _links.TryGetLinksFrom(id, port, out _)) _links.RemovePort(id, port);
            if (!toPortData.IsMultiple() && _links.TryGetLinksTo(toId, toPort, out _)) _links.RemovePort(toId, toPort);

            _links.AddLink(id, port, toId, toPort);

            return true;
        }

        public void RemoveConnection(long id, int port, long toId, int toPort) {
            _links.RemoveLink(id, port, toId, toPort);
        }

        public bool TryGetLinksFrom(long id, int port, out int firstLink) {
            return _links.TryGetLinksFrom(id, port, out firstLink);
        }

        public bool TryGetLinksTo(long id, int port, out int firstLink) {
            return _links.TryGetLinksTo(id, port, out firstLink);
        }

        public bool TryGetNextLink(int previousLink, out int nextLink) {
            return _links.TryGetNextLink(previousLink, out nextLink);
        }

        public void AddPort(long id, int index, Port port) {
            _ports.AddPort(id, index, port);
        }

        public Port GetLinkedPort(int link) {
            ref var l = ref _links.GetLink(link);
            return _ports.TryGetPort(l.nodeId, l.port, out int port)
                ? _ports.GetPortData(port)
                : default;
        }

        public void SetSubgraph(long id, BlueprintAsset2 asset) {
            _subgraphMap[id] = asset;
        }

        public void RemoveSubgraph(long id) {
            _subgraphMap.Remove(id);
        }

        public void InvalidateNode(long id, bool invalidatePorts = false) {

        }

        public void Clear() {
            _onNodeChanged = null;

            _nodeMap.Clear();
            _subgraphMap.Clear();
            _factory.Clear();
            _links.Clear();
            _ports.Clear();
        }

        private void OnPortChanged(long id, int port) {
            BlueprintNodeAddress.Unpack(id, out int sourceId, out _);

            if (_factory.GetSource(sourceId) is IBlueprintConnectionsCallback callback) {
                callback.OnConnectionsChanged(this, id, port);
            }

            _onNodeChanged?.Invoke(id);
        }

/*
#if UNITY_EDITOR



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
