using System.Collections.Generic;
using MisterGames.Common.Data;

namespace MisterGames.Blueprints.Core2 {

    internal sealed class BlueprintCompiler2 {

        private readonly Dictionary<NodeId, NodeId> _runtimeNodeMap = new Dictionary<NodeId, NodeId>();
        private readonly HashSet<NodeId> _compiledNodes = new HashSet<NodeId>();
        private readonly TreeMap<int, RuntimeLink2> _hashLinks = new TreeMap<int, RuntimeLink2>();
        private readonly Dictionary<int, RuntimeLink2> _subgraphRootPorts = new Dictionary<int, RuntimeLink2>();

        public RuntimeBlueprint2 Compile(IBlueprintFactory factory, BlueprintAsset2 asset) {
            var meta = asset.BlueprintMeta;
            var nodeStorage = new RuntimeNodeStorage();
            var linkStorage = new RuntimeLinkStorage(meta.NodeCount, meta.LinkedPortCount, meta.LinkCount);

            _runtimeNodeMap.Clear();
            _compiledNodes.Clear();
            _hashLinks.Clear();

            CompileNodes(factory, asset, nodeStorage, linkStorage, compileExternalPorts: false);
            ConnectHashLinks(linkStorage, _hashLinks);
            InlineLinks(nodeStorage, linkStorage);

            return new RuntimeBlueprint2(factory, nodeStorage, linkStorage);
        }

        public void CompileSubgraph(IBlueprintFactory factory, BlueprintAsset2 asset, BlueprintCompileData data) {
            _runtimeNodeMap.Clear();
            _compiledNodes.Clear();
            _hashLinks.Clear();
            _subgraphRootPorts.Clear();

            AddSubgraphRootNodePorts(data.meta, data.id, data.runtimeId);
            CompileNodes(factory, asset, data.nodeStorage, data.linkStorage, compileExternalPorts: true);
            ConnectHashLinks(data.linkStorage, _hashLinks);
        }

        private void CompileNodes(
            IBlueprintFactory factory,
            BlueprintAsset2 asset,
            IRuntimeNodeStorage nodeStorage,
            IRuntimeLinkStorage linkStorage,
            bool compileExternalPorts
        ) {
            var meta = asset.BlueprintMeta;
            var nodes = meta.Nodes;

            nodeStorage.AllocateSpace(meta.NodeCount);

            foreach (var id in nodes) {
                nodeStorage.AddNode(GetOrCompileNode(factory, asset, nodeStorage, linkStorage, id, compileExternalPorts));
            }
        }

        private NodeId GetOrCompileNode(
            IBlueprintFactory factory,
            BlueprintAsset2 asset,
            IRuntimeNodeStorage nodeStorage,
            IRuntimeLinkStorage linkStorage,
            NodeId id,
            bool compileExternalPorts
        ) {
            if (_compiledNodes.Contains(id)) return _runtimeNodeMap[id];

            var runtimeId = GetOrCreateNode(factory, asset, id);
            var runtimeSource = factory.GetSource(runtimeId.source);

            var meta = asset.BlueprintMeta;
            int portCount = meta.GetPortCount(id);

            linkStorage.SetPortCount(runtimeId.source, runtimeId.node, portCount);

            for (int p = 0; p < portCount; p++) {
                var port = meta.GetPort(id, p);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                PortValidator2.ValidatePort(asset, id, p);
#endif

                bool isExternalPort = port.IsExternal();
                bool isEnterOrOutputPort = port.IsInput() != port.IsData();

                // Port is external: check for links from this port to subgraph root and vice versa.
                if (isExternalPort &&
                    compileExternalPorts &&
                    _subgraphRootPorts.TryGetValue(port.GetSignature(), out var rootPort)
                ) {
                    // External port is enter or output port (link target):
                    // Create link from matched subgraph root port to this external port.
                    if (isEnterOrOutputPort) {
                        int k = linkStorage.SelectPort(rootPort.source, rootPort.node, rootPort.port);
                        linkStorage.InsertLinkAfter(k, runtimeId.source, runtimeId.node, p);
                    }
                    // External port is exit or input port (link owner):
                    // Create link from this external port to matched subgraph root port.
                    else {
                        int k = linkStorage.SelectPort(runtimeId.source, runtimeId.node, p);
                        linkStorage.InsertLinkAfter(k, rootPort.source, rootPort.node, rootPort.port);
                    }
                }

                // Port is enter or output: check for internal links.
                if (isEnterOrOutputPort) {
                    if (runtimeSource is not IBlueprintInternalLink internalLink) continue;

                    int k = linkStorage.SelectPort(runtimeId.source, runtimeId.node, p);
                    internalLink.GetLinkedPorts(id, p, out int s, out int count);

                    for (; s < count; s++) {
                        k = linkStorage.InsertLinkAfter(k, runtimeId.source, runtimeId.node, s);
                    }

                    continue;
                }

                // External ports can not have links to other nodes.
                if (isExternalPort) continue;

                // Port is exit or input: check for links to other nodes.
                int i = linkStorage.SelectPort(runtimeId.source, runtimeId.node, p);

                meta.TryGetLinksFrom(id, p, out int l);
                while (l >= 0) {
                    var link = meta.GetLink(l);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                    LinkValidator2.ValidateLink(asset, id, p, link.id, link.port);
#endif

                    var linkedId = GetOrCreateNode(factory, asset, link.id);
                    i = linkStorage.InsertLinkAfter(i, linkedId.source, linkedId.node, link.port);

                    meta.TryGetNextLink(l, out l);
                }
            }

            if (runtimeSource is IBlueprintHashLink hashLink) {
                AddHashLink(meta, id, runtimeId, hashLink);
            }

            if (runtimeSource is IBlueprintCompiled compiled) {
                compiled.Compile(factory, new BlueprintCompileData(meta, id, runtimeId, nodeStorage, linkStorage));
            }

            _compiledNodes.Add(runtimeId);

            return runtimeId;
        }

        private NodeId GetOrCreateNode(IBlueprintFactory factory, BlueprintAsset2 asset, NodeId id) {
            if (_runtimeNodeMap.TryGetValue(id, out var runtimeId)) return runtimeId;

            var source = asset.BlueprintMeta.GetNodeSource(id);

            int runtimeSourceId = factory.GetOrCreateSource(source.GetType());
            var runtimeSource = factory.GetSource(runtimeSourceId);

            int runtimeNodeId = runtimeSource.AddNodeCopy(source, id.node);
            runtimeId = new NodeId(runtimeSourceId, runtimeNodeId);

            _runtimeNodeMap[id] = runtimeId;

            return runtimeId;
        }

        private void AddSubgraphRootNodePorts(IBlueprintMeta meta, NodeId id, NodeId runtimeId) {
            int rootPortCount = meta.GetPortCount(id);

            for (int p = 0; p < rootPortCount; p++) {
                var port = meta.GetPort(id, p);
                _subgraphRootPorts.Add(port.GetSignature(), new RuntimeLink2(runtimeId.source, runtimeId.node, p));
            }
        }

        private void AddHashLink(IBlueprintMeta meta, NodeId id, NodeId runtimeId, IBlueprintHashLink link) {
            link.GetLinkedPort(id, out int hash, out int port);

            var portData = meta.GetPort(id, port);
            int hashRoot = _hashLinks.GetOrAddNode(hash);

            int dir = portData.IsInput() == portData.IsData() ? 0 : 1;
            int dirRoot = _hashLinks.GetOrAddNode(dir, hashRoot);

            _hashLinks.AddEndPoint(dirRoot, new RuntimeLink2(runtimeId.source, runtimeId.node, port));
        }

        private static void ConnectHashLinks(IRuntimeLinkStorage linkStorage, TreeMap<int, RuntimeLink2> hashLinks) {
            var hashes = hashLinks.Roots;

            foreach (int hash in hashes) {
                int hashRoot = hashLinks.GetNode(hash);
                int fromRoot = hashLinks.GetNode(0, hashRoot);
                int toRoot = hashLinks.GetNode(1, hashRoot);

                int t = hashLinks.GetChild(toRoot);

                for (int f = hashLinks.GetChild(fromRoot); f >= 0; f = hashLinks.GetNext(f)) {
                    var from = hashLinks.GetValueAt(f);
                    int i = linkStorage.SelectPort(from.source, from.node, from.port);

                    for (int l = t; l >= 0; l = hashLinks.GetNext(l)) {
                        var to = hashLinks.GetValueAt(l);
                        i = linkStorage.InsertLinkAfter(i, to.source, to.node, to.port);
                    }
                }
            }
        }

        private static void InlineLinks(IRuntimeNodeStorage nodeStorage, IRuntimeLinkStorage linkStorage) {
            for (int i = 0; i < nodeStorage.Count; i++) {
                var id = nodeStorage.GetNode(i);
                int portCount = linkStorage.GetPortCount(id.source, id.node);

                for (int p = 0; p < portCount; p++) {
                    int l = linkStorage.SelectPort(id.source, id.node, p);

                    while (l >= 0) {
                        var link = linkStorage.GetLink(l);
                        int next = linkStorage.GetNextLink(l);

                        int s = linkStorage.GetFirstLink(link.source, link.node, p);

                        // Linked port has no own links: nothing to inline
                        if (s < 0) {
                            l = next;
                            continue;
                        }

                        // Linked port has own links: inline selected port links
                        // Example: from [0 -> 1, 1 -> 2] to [0 -> 2]:
                        // 1) Remove original link [0 -> 1]
                        // 2) Add inlined link [0 -> 2]
                        // 3) Remove remote link [1 -> 2]
                        // 4) Return to the first inlined links to continue inline checks

                        bool inlined = false;
                        l = linkStorage.RemoveLink(l);

                        while (s >= 0) {
                            link = linkStorage.GetLink(s);
                            l = linkStorage.InsertLinkAfter(l, link.source, link.node, link.port);

                            if (!inlined) {
                                next = l;
                                inlined = true;
                            }

                            int n = linkStorage.GetNextLink(s);
                            linkStorage.RemoveLink(s);

                            s = n;
                        }

                        l = next;
                    }
                }
            }
        }
    }

}
