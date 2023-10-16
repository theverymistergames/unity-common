using System;
using System.Collections.Generic;
using MisterGames.Blueprints.Validation;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public sealed class BlueprintMeta2 : IBlueprintMeta, IComparer<BlueprintLink2> {

        [SerializeField] private SerializedDictionary<long, Vector2> _nodeMap;
        [SerializeField] private SerializedDictionary<long, BlueprintAsset2> _subgraphMap;
        [SerializeField] private BlueprintFactory _factory;
        [SerializeField] private BlueprintLinkStorage _links;
        [SerializeField] private BlueprintPortStorage _ports;

        public IReadOnlyCollection<long> Nodes => _nodeMap.Keys;
        public IReadOnlyCollection<BlueprintAsset2> SubgraphAssets => _subgraphMap.Values;

        private HashSet<long> _changedNodes;

        public void Bind() {
            _changedNodes?.Clear();
            _links.OnPortChanged = OnPortChanged;
        }

        public void Unbind() {
            _changedNodes = null;
            _links.OnPortChanged = null;
        }

        public ReadOnlySpan<long> FlushChanges() {
            if (_changedNodes == null || _changedNodes.Count == 0) return Array.Empty<long>();

            long[] changed = new long[_changedNodes.Count];

            _changedNodes.CopyTo(changed);
            _changedNodes.Clear();

            return changed;
        }

        public Vector2 GetNodePosition(long id) {
            return _nodeMap[id];
        }

        public void SetNodePosition(long id, Vector2 position) {
            if (!_nodeMap.ContainsKey(id)) return;

            _nodeMap[id] = position;
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
            _nodeMap[id] = position;

            source.CreatePorts(this, id);
            source.OnValidate(this, id);

            AddNodeChange(id);

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

        public void InvalidateNode(long id, bool invalidateLinks, bool notify = true) {
            if (!_nodeMap.ContainsKey(id)) return;

            var oldLinksTree = invalidateLinks ? _links.CopyLinks(id) : null;
            if (invalidateLinks) _links.RemoveNode(id);

            var oldPortsTree = _ports.CreatePortSignatureToIndicesTree(id);
            oldPortsTree.AllowDefragmentation(false);

            _ports.RemoveNode(id);

            BlueprintNodeAddress.Unpack(id, out int sourceId, out _);
            _factory.GetSource(sourceId).CreatePorts(this, id);

            bool changed = false;
            _ports.TryGetPorts(id, out int p);

            while (p >= 0) {
                int index = _ports.GetPortKey(p);
                int sign = _ports.GetPort(p).GetSignatureHashCode();

                if (oldPortsTree.TryGetIndex(sign, out int signRoot) &&
                    oldPortsTree.TryGetChildIndex(signRoot, out int pointer)
                ) {
                    if (oldPortsTree.TryGetIndex(index, signRoot, out int pointerToIndex)) {
                        pointer = pointerToIndex;
                    }
                    else {
                        if (invalidateLinks) _links.SetLinks(id, index, oldLinksTree, oldPortsTree.GetKeyAt(pointer));
                        changed = true;
                    }

                    oldPortsTree.RemoveNodeAt(pointer);
                    if (!oldPortsTree.HasChildren(signRoot)) oldPortsTree.RemoveNodeAt(signRoot);
                }
                else {
                    changed = true;
                }

                _ports.TryGetNextPortIndex(p, out p);
            }

            changed |= oldPortsTree.Count > 0;

            if (changed && notify) _changedNodes?.Add(id);
        }

        public bool TryCreateLink(long id, int port, long toId, int toPort) {
            if (id == toId) return false;

            if (!_nodeMap.ContainsKey(id)) return false;
            if (!_nodeMap.ContainsKey(toId)) return false;

            if (!_ports.TryGetPortIndex(id, port, out int portIndex)) return false;
            if (!_ports.TryGetPortIndex(toId, toPort, out int toPortIndex)) return false;

            var portData = _ports.GetPort(portIndex);
            var toPortData = _ports.GetPort(toPortIndex);

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

        public void RemoveLink(long id, int port, long toId, int toPort) {
            _links.RemoveLink(id, port, toId, toPort);
        }

        public bool TryGetLinksFrom(long id, int port, out int firstLink) {
            _links.SortLinksFrom(id, port, this);
            return _links.TryGetLinksFrom(id, port, out firstLink);
        }

        public bool TryGetLinksTo(long id, int port, out int firstLink) {
            return _links.TryGetLinksTo(id, port, out firstLink);
        }

        public bool TryGetNextLink(int previousLink, out int nextLink) {
            return _links.TryGetNextLink(previousLink, out nextLink);
        }

        public void AddPort(long id, Port port) {
            _ports.AddPort(id, port);
        }

        public Port GetLinkedPort(int link) {
            ref var l = ref _links.GetLink(link);
            return _ports.TryGetPortIndex(l.nodeId, l.port, out int port)
                ? _ports.GetPort(port)
                : default;
        }

        public void SetSubgraph(long id, BlueprintAsset2 asset) {
            _subgraphMap[id] = asset;
        }

        public void RemoveSubgraph(long id) {
            _subgraphMap.Remove(id);
        }

        public void Clear() {
            _nodeMap.Clear();
            _subgraphMap.Clear();
            _factory.Clear();
            _links.Clear();
            _ports.Clear();
            _changedNodes = null;
        }

        private void OnPortChanged(long id, int port) {
            BlueprintNodeAddress.Unpack(id, out int sourceId, out _);

            if (_factory.GetSource(sourceId) is IBlueprintConnectionsCallback callback) {
                callback.OnLinksChanged(this, id, port);
            }

            AddNodeChange(id);
        }

        private void AddNodeChange(long id) {
            _changedNodes ??= new HashSet<long>();
            _changedNodes.Add(id);
        }

        int IComparer<BlueprintLink2>.Compare(BlueprintLink2 x, BlueprintLink2 y) {
            return x.nodeId == y.nodeId ? x.port.CompareTo(y.port) : _nodeMap[x.nodeId].y.CompareTo(_nodeMap[y.nodeId].y);
        }
    }

}
