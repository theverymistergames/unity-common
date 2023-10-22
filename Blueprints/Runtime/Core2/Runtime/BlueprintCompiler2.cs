using System;
using System.Collections.Generic;
using MisterGames.Blueprints.Compile;
using MisterGames.Blueprints.Core;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Validation;
using MisterGames.Common.Data;

namespace MisterGames.Blueprints.Core2 {

    internal sealed class BlueprintCompiler2 {

        private readonly Dictionary<NodeId, NodeId> _createdNodes = new Dictionary<NodeId, NodeId>();
        private readonly HashSet<NodeId> _compiledNodes = new HashSet<NodeId>();
        private readonly TreeMap<int, RuntimeLink2> _nodeLinkerTree = new TreeMap<int, RuntimeLink2>();

        private readonly Dictionary<int, List<BlueprintLink>> _externalPortLinksMap = new Dictionary<int, List<BlueprintLink>>();

        public RuntimeBlueprint2 Compile(IBlueprintFactory factory, BlueprintAsset2 asset) {
            var meta = asset.BlueprintMeta;
            int nodeCount = meta.NodeCount;

            var nodeStorage = new RuntimeNodeStorage(nodeCount);
            var linkStorage = new RuntimeLinkStorage(nodeCount, meta.LinkedPortCount, meta.LinkCount);

            _createdNodes.Clear();
            _compiledNodes.Clear();
            _nodeLinkerTree.Clear();

            var nodes = meta.Nodes;

            foreach (var id in nodes) {
                nodeStorage.AddNode(GetOrCompileNode(factory, asset, nodeStorage, linkStorage, id));
            }

            CreateConnectionsBetweenNodeLinkers(linkStorage);

            for (int i = 0; i < nodeStorage.Count; i++) {
                InlineLinks(factory, linkStorage, nodeStorage.GetNode(i));
            }

            return new RuntimeBlueprint2(factory, nodeStorage, linkStorage);
        }

        private NodeId GetOrCompileNode(
            IBlueprintFactory factory,
            BlueprintAsset2 asset,
            IRuntimeNodeStorage nodeStorage,
            IRuntimeLinkStorage linkStorage,
            NodeId id
        ) {
            if (_compiledNodes.Contains(id)) return _createdNodes[id];

            var runtimeId = GetOrCreateNode(factory, asset, id);
            var runtimeSource = factory.GetSource(runtimeId.source);

            var meta = asset.BlueprintMeta;
            int portCount = meta.GetPortCount(id);

            for (int p = 0; p < portCount; p++) {
                var port = meta.GetPort(id, p);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                PortValidator2.ValidatePort(asset, id, p);
#endif

                // Skip enter or data-based output ports
                if (port.IsInput() != port.IsData()) continue;

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

            if (runtimeSource is IBlueprintNodeLinker2 nodeLinker) {
                AddNodeLinker(meta, id, runtimeId, nodeLinker);
            }

            if (runtimeSource is IBlueprintCompiled callback) {
                callback.OnCompile(meta, id, new BlueprintCompileData(factory, nodeStorage, linkStorage));
            }

            _compiledNodes.Add(runtimeId);

            return runtimeId;
        }

        private NodeId GetOrCreateNode(IBlueprintFactory factory, BlueprintAsset2 asset, NodeId id) {
            if (_createdNodes.TryGetValue(id, out var runtimeId)) return runtimeId;

            var source = asset.BlueprintMeta.GetNodeSource(id);

            int runtimeSourceId = factory.GetOrCreateSource(source.GetType());
            var runtimeSource = factory.GetSource(runtimeSourceId);

            int runtimeNodeId = runtimeSource.AddNodeCopy(source, id.node);
            runtimeId = new NodeId(runtimeSourceId, runtimeNodeId);

            _createdNodes[id] = runtimeId;

            return runtimeId;
        }

        public RuntimeBlueprint2 CompileSubgraph(
            IBlueprintMeta rootMeta,
            NodeId rootNodeId,
            BlueprintAsset2 subgraphAsset,
            BlueprintCompileData data
        ) {
            var blueprintMeta = subgraphAsset.BlueprintMeta;
            var nodeIds = blueprintMeta.NodesMap.Keys;

            int nodesCount = nodeIds.Count;
            var runtimeNodes = nodesCount > 0 ? new BlueprintNode[nodesCount] : Array.Empty<BlueprintNode>();

            _runtimeNodesMap.Clear();
            _externalPortLinksMap.Clear();
            _nodeLinkerGroupsMap.Clear();

            int nodeIndex = 0;
            foreach (int nodeId in nodeIds) {
                runtimeNodes[nodeIndex++] = GetOrCompileSubgraphNode(subgraphAsset, subgraphNode, subgraphPorts, nodeId);
            }

            int subgraphPortsCount = subgraphPorts.Length;
            subgraphNode.Ports ??= subgraphPortsCount > 0 ? new RuntimePort[subgraphPortsCount] : Array.Empty<RuntimePort>();

            // Create links from owner subgraph node ports to external nodes inside subgraph
            for (int p = 0; p < subgraphPortsCount; p++) {
                var port = subgraphPorts[p];

                // Skip exit or data-based input ports
                if (port.IsInput() == port.IsData()) continue;

                if (!_externalPortLinksMap.ContainsKey(port.GetSignatureHashCode())) {
                    Debug.LogError($"Subgraph blueprint asset `{subgraphAsset}` in node {subgraphNode} has error: " +
                                   $"external port links not found for subgraph port {port}.");
                    continue;
                }

                var links = _externalPortLinksMap[port.GetSignatureHashCode()];
                int linksCount = links.Count;
                var runtimeLinks = new List<RuntimeLink>(linksCount);

                for (int l = 0; l < linksCount; l++) {
                    var link = links[l];
                    var linkedRuntimeNode = GetOrCompileSubgraphNode(subgraphAsset, subgraphNode, subgraphPorts, link.nodeId);

                    runtimeLinks.Add(new RuntimeLink(linkedRuntimeNode, link.portIndex));
                }

                subgraphNode.Ports[p] = new RuntimePort(runtimeLinks);
            }

            CreateConnectionsBetweenNodeLinkers(subgraphAsset);

            _externalPortLinksMap.Clear();
            _runtimeNodesMap.Clear();
            _nodeLinkerGroupsMap.Clear();

            return new RuntimeBlueprint(runtimeNodes);
        }

        private BlueprintNode GetOrCompileSubgraphNode(
            BlueprintAsset subgraphBlueprint,
            BlueprintNode subgraphNode,
            Port[] subgraphPorts,
            int nodeId
        ) {
            if (_runtimeNodesMap.TryGetValue(nodeId, out var runtimeNode)) return runtimeNode;

            var blueprintMeta = subgraphBlueprint.BlueprintMeta;
            var nodesMetaMap = blueprintMeta.NodesMap;
            var nodeMeta = nodesMetaMap[nodeId];

            runtimeNode = nodeMeta.CreateNodeInstance();
            _runtimeNodesMap[nodeId] = runtimeNode;

            if (runtimeNode is IBlueprintNodeLinker linker) AddNodeLinker(nodeId, linker);
            if (runtimeNode is IBlueprintCompiledNode compiledNode) compiledNode.Compile(nodeMeta.Ports);

            var ports = nodeMeta.Ports;
            int portsCount = ports.Length;
            runtimeNode.Ports ??= portsCount > 0 ? new RuntimePort[portsCount] : Array.Empty<RuntimePort>();

            int subgraphPortsCount = subgraphPorts.Length;

            for (int p = 0; p < portsCount; p++) {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                PortValidator.ValidatePort(subgraphBlueprint, nodeMeta, p);
#endif

                var port = ports[p];
                if (port.IsExternal()) {
                    int portSignature = port.GetSignatureHashCode();

                    // External port is enter or output: add port address as link into external ports map
                    // to create links from matched subgraph node port to this external port.
                    if (port.IsInput() != port.IsData()) {
                        var link = new BlueprintLink { nodeId = nodeId, portIndex = p };

                        if (_externalPortLinksMap.TryGetValue(portSignature, out var externalPortLinks)) {
                            externalPortLinks.Add(link);
                        }
                        else {
                            _externalPortLinksMap[portSignature] = new List<BlueprintLink> { link };
                        }

                        continue;
                    }

                    // External port is exit or input: create link from external port to matched subgraph node port.
                    int subgraphPortIndex = -1;
                    for (int i = 0; i < subgraphPortsCount; i++) {
                        if (subgraphPorts[i].GetSignatureHashCode() != portSignature) continue;

                        subgraphPortIndex = i;
                        break;
                    }

                    runtimeNode.Ports[p] = new RuntimePort(
                        new List<RuntimeLink>(1) { new RuntimeLink(subgraphNode, subgraphPortIndex) }
                    );
                    continue;
                }

                // Skip enter or data-based output ports
                if (port.IsInput() != port.IsData()) continue;

                var links = blueprintMeta.GetLinksFromNodePort(nodeId, p);
                int linksCount = links.Count;
                var runtimeLinks = new List<RuntimeLink>(linksCount);

                for (int l = 0; l < linksCount; l++) {
                    var link = links[l];

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                    LinkValidator.ValidateLink(subgraphBlueprint, nodeMeta, p, nodesMetaMap[link.nodeId], link.portIndex);
#endif

                    var linkedNode = GetOrCompileSubgraphNode(subgraphBlueprint, subgraphNode, subgraphPorts, link.nodeId);
                    runtimeLinks.Add(new RuntimeLink(linkedNode, link.portIndex));
                }

                runtimeNode.Ports[p] = new RuntimePort(runtimeLinks);
            }

            return runtimeNode;
        }

        private void AddNodeLinker(IBlueprintMeta meta, NodeId id, NodeId runtimeId, IBlueprintNodeLinker2 linker) {
            linker.GetLinkedNode(id, out int hash, out int port);

            var portData = meta.GetPort(id, port);
            int hashRoot = _nodeLinkerTree.GetOrAddNode(hash);

            int dir = portData.IsInput() == portData.IsData() ? 0 : 1;
            int dirRoot = _nodeLinkerTree.GetOrAddNode(dir, hashRoot);

            _nodeLinkerTree.AddEndPoint(dirRoot, new RuntimeLink2(runtimeId.source, runtimeId.node, port));
        }

        private void CreateConnectionsBetweenNodeLinkers(IRuntimeLinkStorage linkStorage) {
            var hashes = _nodeLinkerTree.Roots;

            foreach (int hash in hashes) {
                int hashRoot = _nodeLinkerTree.GetNode(hash);
                int fromRoot = _nodeLinkerTree.GetNode(0, hashRoot);
                int toRoot = _nodeLinkerTree.GetNode(1, hashRoot);

                int f = _nodeLinkerTree.GetChild(fromRoot);
                int t = _nodeLinkerTree.GetChild(toRoot);

                while (f >= 0) {
                    var from = _nodeLinkerTree.GetValueAt(f);
                    int i = linkStorage.SelectPort(from.source, from.node, from.port);

                    int l = t;
                    while (l >= 0) {
                        var to = _nodeLinkerTree.GetValueAt(l);
                        i = linkStorage.InsertLinkAfter(i, to.source, to.node, to.port);
                        l = _nodeLinkerTree.GetNext(l);
                    }

                    f = _nodeLinkerTree.GetNext(f);
                }
            }
        }

        private static void InlineLinks(IBlueprintFactory factory, IRuntimeLinkStorage linkStorage, NodeId id) {
            int portCount = linkStorage.GetPortCount(id.source, id.node);

            for (int p = 0; p < portCount; p++) {
                int i = linkStorage.SelectPort(id.source, id.node, p);

                while (i >= 0) {
                    var link = linkStorage.GetLink(i);
                    if (factory.GetSource(link.source) is not IBlueprintPortLinker2 linker) continue;

                    int next = linkStorage.GetNextLink(i);
                    int firstInsertedLink = -1;
                    i = linkStorage.RemoveLink(i);

                    linker.GetLinkedPorts(new NodeId(link.source, link.node), p, out int s, out int count);

                    for (; s < count; s++) {
                        int firstLink = linkStorage.GetFirstLink(link.source, link.node, s);

                        for (int l = firstLink; l >= 0; l = linkStorage.GetNextLink(l)) {
                            var remoteLink = linkStorage.GetLink(l);
                            i = linkStorage.InsertLinkAfter(i, remoteLink.source, remoteLink.node, remoteLink.port);

                            if (firstInsertedLink < 0) firstInsertedLink = i;
                        }

                        linkStorage.RemovePort(link.source, link.node, s);
                    }

                    i = firstInsertedLink >= 0 ? firstInsertedLink : next;
                }
            }
        }
    }

}
