using System;
using System.Collections.Generic;
using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Factory;
using MisterGames.Blueprints.Nodes;
using MisterGames.Blueprints.Validation;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Meta {

    [Serializable]
    public sealed class BlueprintMeta2 : IBlueprintMeta, IComparer<BlueprintLink2> {

        [SerializeField] private Map<NodeId, Vector2> _nodeMap;
        [SerializeField] private Map<NodeId, BlueprintAsset2> _subgraphMap;
        [SerializeField] private BlueprintFactory _factory;
        [SerializeField] private BlueprintLinkStorage _linkStorage;
        [SerializeField] private BlueprintPortStorage _portStorage;

        public IReadOnlyCollection<NodeId> Nodes => _nodeMap.Keys;
        public IDictionary<NodeId, BlueprintAsset2> SubgraphAssetMap => _subgraphMap;

        public int NodeCount => _nodeMap.Count;
        public int LinkCount => _linkStorage.LinkCount;
        public int LinkedPortCount => _linkStorage.LinkedPortCount;

        public readonly Dictionary<NodeId, string> NodeJsonMap = new Dictionary<NodeId, string>();
        public IBlueprintFactory Factory => _factory;

        private Action<NodeId> _onNodeChange;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        internal object owner;
#endif

        public BlueprintMeta2() {
            _nodeMap = new Map<NodeId, Vector2>();
            _subgraphMap = new Map<NodeId, BlueprintAsset2>();
            _factory = new BlueprintFactory();
            _linkStorage = new BlueprintLinkStorage();
            _portStorage = new BlueprintPortStorage();
        }

        public BlueprintMeta2(BlueprintMeta2 source) {
            _nodeMap = new Map<NodeId, Vector2>(source._nodeMap);
            _subgraphMap = new Map<NodeId, BlueprintAsset2>(source._subgraphMap);
            _factory = new BlueprintFactory(source._factory);
            _linkStorage = new BlueprintLinkStorage(source._linkStorage);
            _portStorage = new BlueprintPortStorage(source._portStorage);
        }

        public void Bind(Action<NodeId> onNodeChange) {
            _onNodeChange = onNodeChange;
            _linkStorage.OnPortChanged = OnPortChanged;
        }

        public void Unbind() {
            _onNodeChange = null;
            _linkStorage.OnPortChanged = null;
        }

        public Vector2 GetNodePosition(NodeId id) {
            return _nodeMap[id];
        }

        public void SetNodePosition(NodeId id, Vector2 position) {
            if (_nodeMap.ContainsKey(id)) _nodeMap[id] = position;
        }

        public bool TryGetNodePath(NodeId id, out int sourceIndex, out int nodeIndex) {
            return _factory.TryGetNodePath(id, out sourceIndex, out nodeIndex);
        }

        public bool ContainsNode(NodeId id) {
            return _nodeMap.ContainsKey(id);
        }

        public NodeId AddNode(Type sourceType, Type nodeType, Vector2 position = default) {
            int sourceId = _factory.GetOrCreateSource(sourceType);
            var source = _factory.GetSource(sourceId);

            int nodeId = source.AddNode(nodeType);
            var id = new NodeId(sourceId, nodeId);

            _nodeMap[id] = position;

            source.CreatePorts(this, id);
            source.OnSetDefaults(this, id);
            source.OnValidate(this, id);

            _onNodeChange?.Invoke(id);

            return id;
        }

        public bool RemoveNode(NodeId id) {
            if (!_nodeMap.ContainsKey(id)) return false;

            bool removed = _nodeMap.Remove(id);
            _subgraphMap.Remove(id);
            _linkStorage.RemoveNode(id);
            _portStorage.RemoveNode(id);

            var source = _factory.GetSource(id.source);
            source?.RemoveNode(id.node);

            if (source == null || source.Count == 0) {
                source?.Clear();
                _factory.RemoveSource(id.source);
            }

            if (_nodeMap.Count == 0) Clear();
            else if (_linkStorage.LinkCount == 0) _linkStorage.Clear();

            _onNodeChange?.Invoke(id);
            return removed;
        }

        public bool InvalidateNode(NodeId id, bool invalidateLinks, bool notify = true) {
            if (!_nodeMap.ContainsKey(id)) return false;

            var source = _factory.GetSource(id.source);
            if (source == null) return false;

            var oldPortsTree = _portStorage.CreatePortSignatureToIndicesTree(id);
            oldPortsTree?.AllowDefragmentation(false);

            _portStorage.RemoveNode(id);
            source.CreatePorts(this, id);

            TreeSet<BlueprintLink2> oldLinksTree = null;
            if (invalidateLinks) {
                oldLinksTree = _linkStorage.CopyLinks(id);
                _linkStorage.RemoveNode(id, notify: false);
            }

            bool changed = false;
            int portCount = _portStorage.GetPortCount(id);

            for (int i = 0; i < portCount; i++) {
                _portStorage.TryGetPort(id, i, out var port);
                int sign = port.GetSignature();

                if (oldPortsTree != null &&
                    oldPortsTree.TryGetNode(sign, out int signRoot) &&
                    oldPortsTree.TryGetChild(signRoot, out int pointer)
                ) {
                    if (oldPortsTree.TryGetNode(i, signRoot, out int p)) pointer = p;
                    else changed = true;

                    if (invalidateLinks) {
                        _linkStorage.SetLinks(id, i, oldLinksTree, oldPortsTree.GetKeyAt(pointer), notify: false);
                    }

                    oldPortsTree.RemoveNodeAt(pointer);
                    if (!oldPortsTree.ContainsChildren(signRoot)) oldPortsTree.RemoveNodeAt(signRoot);

                    continue;
                }

                changed = true;
            }

            changed |= oldPortsTree is { Count: > 0 };
            if (changed && notify) _onNodeChange?.Invoke(id);

            return changed;
        }

        public bool TryCreateLink(NodeId id, int port, NodeId toId, int toPort) {
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
                var t0 = id;
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

            if (!portData.IsMultiple()) _linkStorage.RemovePort(id, port);
            if (!toPortData.IsMultiple()) _linkStorage.RemovePort(toId, toPort);

            _linkStorage.AddLink(id, port, toId, toPort);

            return true;
        }

        public bool RemoveLink(NodeId id, int port, NodeId toId, int toPort) {
            bool removed = _linkStorage.RemoveLink(id, port, toId, toPort);
            if (_linkStorage.LinkCount == 0) _linkStorage.Clear();

            return removed;
        }

        public IBlueprintSource GetNodeSource(NodeId id) {
            return _nodeMap.ContainsKey(id) ? _factory.GetSource(id.source) : null;
        }

        public Blackboard GetBlackboard() {
#if UNITY_EDITOR
            return owner switch {
                BlueprintAsset2 asset => asset.Blackboard,
                BlueprintRunner2 runner => runner.GetRootBlackboard(),
                VirtualBlueprintContainer container => container.Blackboard,
                _ => null,
            };
#endif

            return null;
        }

        public Port GetPort(NodeId id, int port) {
            _portStorage.TryGetPort(id, port, out var data);
            return data;
        }

        public void AddPort(NodeId id, Port port) {
            if (_nodeMap.ContainsKey(id)) _portStorage.AddPort(id, port);
        }

        public int GetPortCount(NodeId id) {
            return _portStorage.GetPortCount(id);
        }

        public BlueprintLink2 GetLink(int index) {
            return _linkStorage.GetLink(index);
        }

        public bool TryGetLinksFrom(NodeId id, int port, out int index) {
            _linkStorage.SortLinksFrom(id, port, this);
            return _linkStorage.TryGetLinksFrom(id, port, out index);
        }

        public bool TryGetLinksTo(NodeId id, int port, out int index) {
            return _linkStorage.TryGetLinksTo(id, port, out index);
        }

        public bool TryGetNextLink(int previous, out int next) {
            return _linkStorage.TryGetNextLink(previous, out next);
        }

        public void SetSubgraph(NodeId id, BlueprintAsset2 asset) {
            _subgraphMap[id] = asset;
        }

        public void RemoveSubgraph(NodeId id) {
            _subgraphMap.Remove(id);
        }

        public void Clear() {
            _nodeMap.Clear();
            _subgraphMap.Clear();
            _factory.Clear();
            _linkStorage.Clear();
            _portStorage.Clear();
        }

        private void OnPortChanged(NodeId id, int port) {
            if (_factory.GetSource(id.source) is IBlueprintConnectionCallback callback) {
                callback.OnLinksChanged(this, id, port);
            }

            _onNodeChange?.Invoke(id);
        }

        int IComparer<BlueprintLink2>.Compare(BlueprintLink2 x, BlueprintLink2 y) {
            return x.id == y.id ? x.port.CompareTo(y.port) : _nodeMap[x.id].y.CompareTo(_nodeMap[y.id].y);
        }

        public override string ToString() {
            return $"{nameof(BlueprintMeta2)}(owner {owner})";
        }
    }

}
