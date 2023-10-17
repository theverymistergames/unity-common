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
        [SerializeField] private BlueprintLinkStorage _linkStorage;
        [SerializeField] private BlueprintPortStorage _portStorage;

        public IReadOnlyCollection<long> Nodes => _nodeMap.Keys;
        public IReadOnlyCollection<BlueprintAsset2> SubgraphAssets => _subgraphMap.Values;

        private Action<long> _onNodeChange;

        public BlueprintMeta2() {
            _nodeMap = new SerializedDictionary<long, Vector2>();
            _subgraphMap = new SerializedDictionary<long, BlueprintAsset2>();
            _factory = new BlueprintFactory();
            _linkStorage = new BlueprintLinkStorage();
            _portStorage = new BlueprintPortStorage();
        }

        public void Bind(Action<long> onNodeChange) {
            _onNodeChange = onNodeChange;
            _linkStorage.OnPortChanged = OnPortChanged;
        }

        public void Unbind() {
            _onNodeChange = null;
            _linkStorage.OnPortChanged = null;
        }

        public Vector2 GetNodePosition(long id) {
            return _nodeMap[id];
        }

        public void SetNodePosition(long id, Vector2 position) {
            if (_nodeMap.ContainsKey(id)) _nodeMap[id] = position;
        }

        public string GetNodePath(long id) {
            BlueprintNodeAddress.Unpack(id, out int sourceId, out int nodeId);
            return $"{nameof(_factory)}.{_factory.GetSourcePath(sourceId)}.{_factory.GetSource(sourceId).GetNodePath(nodeId)}";
        }

        public bool ContainsNode(long id) {
            return _nodeMap.ContainsKey(id);
        }

        public long AddNode(Type sourceType, Vector2 position) {
            int sourceId = _factory.GetOrCreateSource(sourceType);
            var source = _factory.GetSource(sourceId);
            int nodeId = source.AddNode();

            long id = BlueprintNodeAddress.Pack(sourceId, nodeId);
            _nodeMap[id] = position;

            source.CreatePorts(this, id);
            source.SetDefaultValues(this, id);
            source.OnValidate(this, id);

            _onNodeChange?.Invoke(id);

            return id;
        }

        public void RemoveNode(long id) {
            if (!_nodeMap.ContainsKey(id)) return;

            _nodeMap.Remove(id);
            _subgraphMap.Remove(id);
            _linkStorage.RemoveNode(id);
            _portStorage.RemoveNode(id);

            BlueprintNodeAddress.Unpack(id, out int sourceId, out int nodeId);

            var source = _factory.GetSource(sourceId);
            source.RemoveNode(nodeId);
            if (source.Count == 0) _factory.RemoveSource(sourceId);

            _onNodeChange?.Invoke(id);
        }

        public void InvalidateNode(long id, bool invalidateLinks, bool notify = true) {
            if (!_nodeMap.ContainsKey(id)) return;

            var oldLinksTree = invalidateLinks ? _linkStorage.CopyLinks(id) : null;
            if (invalidateLinks) _linkStorage.RemoveNode(id);

            var oldPortsTree = _portStorage.CreatePortSignatureToIndicesTree(id);
            oldPortsTree.AllowDefragmentation(false);

            _portStorage.RemoveNode(id);

            BlueprintNodeAddress.Unpack(id, out int sourceId, out _);
            _factory.GetSource(sourceId).CreatePorts(this, id);

            bool changed = false;
            int portCount = _portStorage.GetPortCount(id);

            for (int i = 0; i < portCount; i++) {
                _portStorage.TryGetPort(id, i, out var port);
                int sign = port.GetSignature();

                if (oldPortsTree.TryGetIndex(sign, out int signRoot) &&
                    oldPortsTree.TryGetChildIndex(signRoot, out int pointer)
                ) {
                    if (oldPortsTree.TryGetIndex(i, signRoot, out int p)) pointer = p;
                    else changed = true;

                    if (invalidateLinks) _linkStorage.SetLinks(id, i, oldLinksTree, oldPortsTree.GetKeyAt(pointer));

                    oldPortsTree.RemoveNodeAt(pointer);
                    if (!oldPortsTree.HasChildren(signRoot)) oldPortsTree.RemoveNodeAt(signRoot);
                }
                else {
                    changed = true;
                }
            }

            changed |= oldPortsTree.Count > 0;

            if (changed && notify) _onNodeChange?.Invoke(id);
        }

        public bool TryCreateLink(long id, int port, long toId, int toPort) {
            if (id == toId) return false;

            if (!_nodeMap.ContainsKey(id)) return false;
            if (!_nodeMap.ContainsKey(toId)) return false;

            if (!_portStorage.TryGetPort(id, port, out var portData)) return false;
            if (!_portStorage.TryGetPort(toId, toPort, out var toPortData)) return false;

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

            if (_linkStorage.ContainsLink(id, port, toId, toPort)) return false;

            if (!portData.IsMultiple() && _linkStorage.TryGetLinksFrom(id, port, out _)) {
                _linkStorage.RemovePort(id, port);
            }

            if (!toPortData.IsMultiple() && _linkStorage.TryGetLinksTo(toId, toPort, out _)) {
                _linkStorage.RemovePort(toId, toPort);
            }

            _linkStorage.AddLink(id, port, toId, toPort);

            return true;
        }

        public void RemoveLink(long id, int port, long toId, int toPort) {
            _linkStorage.RemoveLink(id, port, toId, toPort);
        }

        public Port GetPort(long id, int port) {
            _portStorage.TryGetPort(id, port, out var data);
            return data;
        }

        public void AddPort(long id, Port port) {
            if (_nodeMap.ContainsKey(id)) _portStorage.AddPort(id, port);
        }

        public int GetPortCount(long id) {
            return _portStorage.GetPortCount(id);
        }

        public BlueprintLink2 GetLink(int index) {
            return _linkStorage.GetLink(index);
        }

        public bool TryGetLinksFrom(long id, int port, out int index) {
            _linkStorage.SortLinksFrom(id, port, this);
            return _linkStorage.TryGetLinksFrom(id, port, out index);
        }

        public bool TryGetLinksTo(long id, int port, out int index) {
            return _linkStorage.TryGetLinksTo(id, port, out index);
        }

        public bool TryGetNextLink(int previous, out int next) {
            return _linkStorage.TryGetNextLink(previous, out next);
        }

        public void SetSubgraph(long id, BlueprintAsset2 asset) {
            if (_nodeMap.ContainsKey(id)) _subgraphMap[id] = asset;
        }

        public void RemoveSubgraph(long id) {
            _subgraphMap.Remove(id);
        }

        public void Clear() {
            _nodeMap.Clear();
            _subgraphMap.Clear();
            _factory.Clear();
            _linkStorage.Clear();
            _portStorage.Clear();
        }

        private void OnPortChanged(long id, int port) {
            BlueprintNodeAddress.Unpack(id, out int sourceId, out _);

            if (_factory.GetSource(sourceId) is IBlueprintConnectionsCallback callback) {
                callback.OnLinksChanged(this, id, port);
            }

            _onNodeChange?.Invoke(id);
        }

        int IComparer<BlueprintLink2>.Compare(BlueprintLink2 x, BlueprintLink2 y) {
            return x.nodeId == y.nodeId ? x.port.CompareTo(y.port) : _nodeMap[x.nodeId].y.CompareTo(_nodeMap[y.nodeId].y);
        }

        public override string ToString() {
            return $"{nameof(BlueprintMeta2)}:\n" +
                   $"Nodes: [{string.Join("] [", _nodeMap.Keys)}]\n" +
                   $"Links: {_linkStorage}" +
                   $"Ports: {_portStorage}";
        }
    }

}
