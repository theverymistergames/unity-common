using System.Collections.Generic;
using MisterGames.Blueprints.Factory;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Nodes;
using MisterGames.Blueprints.Validation;
using MisterGames.Common.Data;

namespace MisterGames.Blueprints.Runtime {

    internal sealed class BlueprintCompiler2 {

        private readonly Dictionary<NodeId, NodeId> _runtimeNodeMap = new Dictionary<NodeId, NodeId>();
        private readonly TreeMap<int, RuntimeLink2> _hashLinks = new TreeMap<int, RuntimeLink2>();

        public RuntimeBlueprint2 Compile(BlueprintMeta2 meta, IBlueprintFactory factory, IBlueprintHost2 host) {
            _runtimeNodeMap.Clear();
            _hashLinks.Clear();

            // Create root node
            int rootSourceId = factory.GetOrCreateSource(typeof(BlueprintSourceRoot));
            int rootNodeId = factory.GetSource(rootSourceId).AddNode();
            var root = new NodeId(rootSourceId, rootNodeId);

#if UNITY_EDITOR
            meta.NodeJsonMap.Clear();
#endif

            var nodeStorage = new RuntimeNodeStorage(meta.NodeCount + 1);
            var linkStorage = new RuntimeLinkStorage(meta.LinkedPortCount, meta.LinkCount);
            var blackboardStorage = new RuntimeBlackboardStorage(meta.SubgraphAssetMap.Count + 1);
            var blueprint = new RuntimeBlueprint2(root, factory, nodeStorage, linkStorage, blackboardStorage);

            blackboardStorage.SetBlackboard(root, host.GetRootBlackboard());
            CompileNodes(host, host.GetRootFactory(), meta, blueprint, root);
            linkStorage.InlineLinks();

            return blueprint;
        }

        public void CompileSubgraph(BlueprintMeta2 meta, SubgraphCompileData data) {
            _runtimeNodeMap.Clear();
            _hashLinks.Clear();

            var host = data.host;
            var blueprint = data.blueprint;
            var factoryOverride = host.GetSubgraphFactory(data.id, data.parent);
            int parent = host.GetSubgraphIndex(data.id, data.parent);

#if UNITY_EDITOR
            meta.NodeJsonMap.Clear();
#endif

            blueprint.blackboardStorage.SetBlackboard(data.runtimeId, host.GetSubgraphBlackboard(data.id, data.parent));
            blueprint.nodeStorage.AllocateNodes(meta.NodeCount);

            CompileNodes(host, factoryOverride, meta, blueprint, data.runtimeId, parent);
        }

        private void CompileNodes(
            IBlueprintHost2 host,
            IBlueprintFactory factoryOverride,
            BlueprintMeta2 meta,
            RuntimeBlueprint2 blueprint,
            NodeId root,
            int parent = -1
        ) {
            var factory = blueprint.factory;
            var nodeStorage = blueprint.nodeStorage;
            var linkStorage = blueprint.linkStorage;

            var nodes = meta.Nodes;

            foreach (var id in nodes) {
                var source = meta.GetNodeSource(id);

                // Override node source if factory override contains node
                if (factoryOverride?.GetSource(id.source) is { } sourceOverride &&
                    sourceOverride.ContainsNode(id.node)
                ) {
                    source = sourceOverride;
                }

                // Try create node
                if (!_runtimeNodeMap.TryGetValue(id, out var runtimeId)) {
                    if (source == null) continue;

                    int runtimeSourceId = factory.GetOrCreateSource(source.GetType());
                    var runtimeSource = factory.GetSource(runtimeSourceId);
                    int runtimeNodeId;

                    if (source is IBlueprintCloneable) {
                        runtimeNodeId = runtimeSource.AddNodeClone(source, id.node);
                    }
                    else {
                        if (!meta.NodeJsonMap.TryGetValue(id, out string json)) {
                            json = source.GetNodeAsString(id.node);
                            meta.NodeJsonMap[id] = json;
                        }
                        runtimeNodeId = runtimeSource.AddNodeFromString(json, source.GetNodeType(id.node));
                    }

                    runtimeId = new NodeId(runtimeSourceId, runtimeNodeId);
                    _runtimeNodeMap[id] = runtimeId;
                }

                int portCount = meta.GetPortCount(id);
                bool hasSignaturePorts = source is IBlueprintCreateSignaturePorts c && c.HasSignaturePorts(id);

                for (int p = 0; p < portCount; p++) {
                    var port = meta.GetPort(id, p);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                    PortValidator2.ValidatePort(meta, id, p);
#endif

                    int sign = port.GetSignature();
                    bool isExternal = port.IsExternal();
                    bool isEnterOrOutput = port.IsInput() != port.IsData();

                    if (isExternal) {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                        if (root == blueprint.root) blueprint.rootPorts[sign] = port;
                        LinkValidator2.ValidateRootLink(meta, linkStorage, id, p, root, sign);
#endif

                        // Compile links between root and external ports
                        if (isEnterOrOutput) {
                            // External port is enter or output port (link target):
                            // Create link from matched subgraph root port to this external port.
                            int i = linkStorage.SelectPort(root.source, root.node, sign);
                            linkStorage.InsertLinkAfter(i, runtimeId.source, runtimeId.node, p);
                        }
                        else {
                            // External port is exit or input port (link owner):
                            // Create link from this external port to matched subgraph root port.
                            int i = linkStorage.SelectPort(runtimeId.source, runtimeId.node, p);
                            linkStorage.InsertLinkAfter(i, root.source, root.node, sign);
                        }
                    }
                    else {
                        // Compile signature ports layer
                        if (hasSignaturePorts) {
                            if (isEnterOrOutput) {
                                int i = linkStorage.SelectPort(runtimeId.source, runtimeId.node, p);
                                linkStorage.InsertLinkAfter(i, runtimeId.source, runtimeId.node, sign);
                            }
                            else {
                                int i = linkStorage.SelectPort(runtimeId.source, runtimeId.node, sign);
                                linkStorage.InsertLinkAfter(i, runtimeId.source, runtimeId.node, p);
                            }
                        }
                    }

                    if (isEnterOrOutput) {
                        // Compile in-node links
                        if (source is IBlueprintInternalLink internalLink) {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                            LinkValidator2.ValidateInternalLink(meta, id, p, internalLink);
#endif

                            internalLink.GetLinkedPorts(id, p, out int s, out int count);
                            int i = linkStorage.SelectPort(runtimeId.source, runtimeId.node, p);

                            for (int end = s + count; s < end; s++) {
                                i = linkStorage.InsertLinkAfter(i, runtimeId.source, runtimeId.node, s);
                            }
                        }

                        continue;
                    }

                    // External ports can not have links to other nodes.
                    if (isExternal) continue;

                    // Compile node links
                    int k = linkStorage.SelectPort(runtimeId.source, runtimeId.node, p);
                    for (meta.TryGetLinksFrom(id, p, out int l); l >= 0; meta.TryGetNextLink(l, out l)) {
                        var link = meta.GetLink(l);
                        var linkedId = link.id;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                        LinkValidator2.ValidateNodeLink(meta, id, p, linkedId, link.port);
#endif

                        if (!_runtimeNodeMap.TryGetValue(linkedId, out var linkedRuntimeId)) {
                            var linkedSource = meta.GetNodeSource(linkedId);
                            if (linkedSource == null) continue;

                            // Override node source if factory override contains node
                            if (factoryOverride?.GetSource(linkedId.source) is { } linkedSourceOverride &&
                                linkedSourceOverride.ContainsNode(linkedId.node)
                            ) {
                                linkedSource = linkedSourceOverride;
                            }

                            int linkedRuntimeSourceId = factory.GetOrCreateSource(linkedSource.GetType());
                            var linkedRuntimeSource = factory.GetSource(linkedRuntimeSourceId);
                            int linkedRuntimeNodeId;

                            if (linkedSource is IBlueprintCloneable) {
                                linkedRuntimeNodeId = linkedRuntimeSource.AddNodeClone(linkedSource, linkedId.node);
                            }
                            else {
                                if (!meta.NodeJsonMap.TryGetValue(linkedId, out string json)) {
                                    json = linkedSource.GetNodeAsString(linkedId.node);
                                    meta.NodeJsonMap[linkedId] = json;
                                }
                                linkedRuntimeNodeId = linkedRuntimeSource.AddNodeFromString(json, linkedSource.GetNodeType(linkedId.node));
                            }

                            linkedRuntimeId = new NodeId(linkedRuntimeSourceId, linkedRuntimeNodeId);
                            _runtimeNodeMap[linkedId] = linkedRuntimeId;
                        }

                        k = linkStorage.InsertLinkAfter(k, linkedRuntimeId.source, linkedRuntimeId.node, link.port);
                    }
                }

                if (source is IBlueprintHashLink hashLink && hashLink.TryGetLinkedPort(id, out int hash, out int linkedPort)) {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                    LinkValidator2.ValidateHashLink(hashLink, meta, id);
#endif

                    var port = meta.GetPort(id, linkedPort);
                    int hashRoot = _hashLinks.GetOrAddNode(hash);

                    int dir = port.IsInput() == port.IsData() ? 0 : 1;
                    int dirRoot = _hashLinks.GetOrAddNode(dir, hashRoot);

                    _hashLinks.AddEndPoint(dirRoot, new RuntimeLink2(runtimeId.source, runtimeId.node, linkedPort));
                }

                if (source is IBlueprintCompilable compilable) {
                    compilable.Compile(id, new SubgraphCompileData(host, blueprint, id, runtimeId, parent));
                }

                nodeStorage.AddNode(runtimeId);
            }

            var hashes = _hashLinks.Roots;
            foreach (int hash in hashes) {
                int hashRoot = _hashLinks.GetNode(hash);
                int fromRoot = _hashLinks.GetNode(0, hashRoot);
                int toRoot = _hashLinks.GetNode(1, hashRoot);

                int t = _hashLinks.GetChild(toRoot);

                for (int f = _hashLinks.GetChild(fromRoot); f >= 0; f = _hashLinks.GetNext(f)) {
                    var from = _hashLinks.GetValueAt(f);
                    int i = linkStorage.SelectPort(from.source, from.node, from.port);

                    for (int l = t; l >= 0; l = _hashLinks.GetNext(l)) {
                        var to = _hashLinks.GetValueAt(l);
                        i = linkStorage.InsertLinkAfter(i, to.source, to.node, to.port);
                    }
                }
            }
        }
    }

}
