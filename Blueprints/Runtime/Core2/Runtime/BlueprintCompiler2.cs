using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MisterGames.Common.Data;

namespace MisterGames.Blueprints.Core2 {

    internal sealed class BlueprintCompiler2 {

        private readonly Dictionary<NodeId, NodeId> _runtimeNodeMap = new Dictionary<NodeId, NodeId>();
        private readonly HashSet<NodeId> _compiledNodes = new HashSet<NodeId>();
        private readonly TreeMap<int, RuntimeLink2> _hashLinks = new TreeMap<int, RuntimeLink2>();
        private readonly Dictionary<int, RuntimeLink2> _subgraphRootPorts = new Dictionary<int, RuntimeLink2>();

        public RuntimeBlueprint2 Compile(IBlueprintFactory factory, BlueprintMeta2 meta) {
            var nodeStorage = new RuntimeNodeStorage();
            var linkStorage = new RuntimeLinkStorage(meta.NodeCount, meta.LinkedPortCount, meta.LinkCount);

            _runtimeNodeMap.Clear();
            _compiledNodes.Clear();
            _hashLinks.Clear();

            CompileNodes(factory, meta, nodeStorage, linkStorage, compileExternalPorts: false);
            BlueprintCompilation.CompileHashLinks(linkStorage, _hashLinks);
            BlueprintCompilation.InlineLinks(nodeStorage, linkStorage);

            return new RuntimeBlueprint2(factory, nodeStorage, linkStorage);
        }

        public void CompileSubgraph(IBlueprintFactory factory, BlueprintMeta2 meta, BlueprintCompileData data) {
            _runtimeNodeMap.Clear();
            _compiledNodes.Clear();
            _hashLinks.Clear();
            _subgraphRootPorts.Clear();

            BlueprintCompilation.FillSignatureToPortMap(_subgraphRootPorts, data.meta, data.id, data.runtimeId);
            CompileNodes(factory, meta, data.nodeStorage, data.linkStorage, compileExternalPorts: true);
            BlueprintCompilation.CompileHashLinks(data.linkStorage, _hashLinks);
        }

        private void CompileNodes(
            IBlueprintFactory factory,
            BlueprintMeta2 meta,
            IRuntimeNodeStorage nodeStorage,
            IRuntimeLinkStorage linkStorage,
            bool compileExternalPorts
        ) {
            var nodes = meta.Nodes;
            nodeStorage.AllocateSpace(meta.NodeCount);

            foreach (var id in nodes) {
                nodeStorage.AddNode(GetOrCompileNode(factory, meta, nodeStorage, linkStorage, id, compileExternalPorts));
            }
        }

        private NodeId GetOrCompileNode(
            IBlueprintFactory factory,
            BlueprintMeta2 meta,
            IRuntimeNodeStorage nodeStorage,
            IRuntimeLinkStorage linkStorage,
            NodeId id,
            bool compileExternalPorts
        ) {
            if (_compiledNodes.Contains(id)) return _runtimeNodeMap[id];

            var runtimeId = GetOrCreateNode(factory, meta, id);
            var runtimeSource = factory.GetSource(runtimeId.source);

            int portCount = meta.GetPortCount(id);

            linkStorage.SetPortCount(runtimeId.source, runtimeId.node, portCount);

            for (int p = 0; p < portCount; p++) {
                var port = meta.GetPort(id, p);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                PortValidator2.ValidatePort(meta, id, p);
#endif

                bool isExternal = port.IsExternal();
                bool isEnterOrOutput = port.IsInput() != port.IsData();

                // Port is external: check for links from this port to subgraph root and vice versa.
                if (isExternal &&
                    compileExternalPorts &&
                    _subgraphRootPorts.TryGetValue(port.GetSignature(), out var rootPort)
                ) {
                    BlueprintCompilation.CompileExternalLinks(linkStorage, rootPort, runtimeId, p, isEnterOrOutput);
                }

                // Port is enter or output: check for internal links.
                if (isEnterOrOutput) {
                    if (runtimeSource is not IBlueprintInternalLink internalLink) continue;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                    LinkValidator2.ValidateInternalLink(meta, id, p, internalLink);
#endif

                    BlueprintCompilation.CompileInternalLink(internalLink, linkStorage, id, runtimeId, p);
                    continue;
                }

                // External ports can not have links to other nodes.
                if (isExternal) continue;

                // Port is exit or input: check for links to other nodes.
                int i = linkStorage.SelectPort(runtimeId.source, runtimeId.node, p);

                for (meta.TryGetLinksFrom(id, p, out int l); l >= 0; meta.TryGetNextLink(l, out l)) {
                    var link = meta.GetLink(l);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                    LinkValidator2.ValidateNodeLink(meta, id, p, link.id, link.port);
#endif

                    var linkedId = GetOrCreateNode(factory, meta, link.id);
                    i = linkStorage.InsertLinkAfter(i, linkedId.source, linkedId.node, link.port);
                }
            }

            if (runtimeSource is IBlueprintHashLink hashLink) {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                LinkValidator2.ValidateHashLink(hashLink, meta, id);
#endif

                BlueprintCompilation.AddHashLink(_hashLinks, meta, id, runtimeId, hashLink);
            }

            if (runtimeSource is IBlueprintCompiled compiled) {
                compiled.Compile(factory, new BlueprintCompileData(meta, id, runtimeId, nodeStorage, linkStorage));
            }

            _compiledNodes.Add(runtimeId);

            return runtimeId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NodeId GetOrCreateNode(IBlueprintFactory factory, BlueprintMeta2 meta, NodeId id) {
            if (_runtimeNodeMap.TryGetValue(id, out var runtimeId)) return runtimeId;

            runtimeId = BlueprintCompilation.CreateNode(factory, meta.GetNodeSource(id), id);
            _runtimeNodeMap[id] = runtimeId;

            return runtimeId;
        }
    }

}
