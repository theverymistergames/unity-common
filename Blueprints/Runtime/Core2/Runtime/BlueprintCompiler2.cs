using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MisterGames.Common.Data;

namespace MisterGames.Blueprints.Core2 {

    internal sealed class BlueprintCompiler2 {

        private readonly Dictionary<NodeId, NodeId> _runtimeNodeMap = new Dictionary<NodeId, NodeId>();
        private readonly HashSet<NodeId> _compiledNodes = new HashSet<NodeId>();
        private readonly TreeMap<int, RuntimeLink2> _hashLinks = new TreeMap<int, RuntimeLink2>();

        public RuntimeBlueprint2 Compile(IBlueprintFactory factory, BlueprintMeta2 meta) {
            _runtimeNodeMap.Clear();
            _compiledNodes.Clear();
            _hashLinks.Clear();

            var root = BlueprintCompilation.CreateRootNode(factory);
            var nodeStorage = new RuntimeNodeStorage(meta.NodeCount + 1);
            var linkStorage = new RuntimeLinkStorage(meta.LinkedPortCount, meta.LinkCount);

            CompileNodes(factory, meta, nodeStorage, linkStorage, root);
            BlueprintCompilation.CompileHashLinks(linkStorage, _hashLinks);

            linkStorage.InlineLinks();

            return new RuntimeBlueprint2(factory, nodeStorage, linkStorage);
        }

        public void CompileSubgraph(BlueprintMeta2 meta, BlueprintCompileData data) {
            _runtimeNodeMap.Clear();
            _compiledNodes.Clear();
            _hashLinks.Clear();

            var root = data.runtimeId;
            var nodeStorage = data.nodeStorage;
            var linkStorage = data.linkStorage;

            nodeStorage.AllocateNodes(meta.NodeCount);

            CompileNodes(data.factory, meta, nodeStorage, linkStorage, root);
            BlueprintCompilation.CompileHashLinks(linkStorage, _hashLinks);
        }

        private void CompileNodes(
            IBlueprintFactory factory,
            BlueprintMeta2 meta,
            IRuntimeNodeStorage nodeStorage,
            IRuntimeLinkStorage linkStorage,
            NodeId rootId
        ) {
            linkStorage.Root = rootId;

            var nodes = meta.Nodes;
            foreach (var id in nodes) {
                nodeStorage.AddNode(GetOrCompileNode(factory, meta, nodeStorage, linkStorage, id, rootId));
            }
        }

        private NodeId GetOrCompileNode(
            IBlueprintFactory factory,
            BlueprintMeta2 meta,
            IRuntimeNodeStorage nodeStorage,
            IRuntimeLinkStorage linkStorage,
            NodeId id,
            NodeId rootId
        ) {
            if (_compiledNodes.Contains(id)) return _runtimeNodeMap[id];

            var runtimeId = GetOrCreateNode(factory, meta, id);
            var runtimeSource = factory.GetSource(runtimeId.source);
            int portCount = meta.GetPortCount(id);

            for (int p = 0; p < portCount; p++) {
                var port = meta.GetPort(id, p);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                PortValidator2.ValidatePort(meta, id, p);
#endif

                int sign = port.GetSignature();
                bool isExternal = port.IsExternal();
                bool isEnterOrOutput = port.IsInput() != port.IsData();

                if (isExternal) {
                    CheckExternalLink(meta, linkStorage, id, runtimeId, p, rootId, sign, isEnterOrOutput);
                }
                else {
                    CheckCompiledNodeLink(runtimeSource, linkStorage, id, p, sign, isEnterOrOutput);
                }

                if (isEnterOrOutput) {
                    CheckInternalLinks(meta, runtimeSource, linkStorage, id, runtimeId, p);
                    continue;
                }

                // External ports can not have links to other nodes.
                if (!isExternal) CheckNodeLinks(factory, meta, linkStorage, id, runtimeId, p);
            }

            CheckHashLink(meta, runtimeSource, id, runtimeId);
            CheckCompiledNode(factory, runtimeSource, nodeStorage, linkStorage, id, runtimeId);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckCompiledNode(
            IBlueprintFactory factory,
            IBlueprintSource source,
            IRuntimeNodeStorage nodeStorage,
            IRuntimeLinkStorage linkStorage,
            NodeId id,
            NodeId runtimeId
        ) {
            if (source is not IBlueprintCompiled subgraph) return;

            subgraph.Compile(id, new BlueprintCompileData(factory, nodeStorage, linkStorage, runtimeId));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckCompiledNodeLink(
            IBlueprintSource source,
            IRuntimeLinkStorage linkStorage,
            NodeId id,
            int port,
            int sign,
            bool isEnterOrOutput
        ) {
            if (source is not IBlueprintCompiled) return;

            if (isEnterOrOutput) {
                int i = linkStorage.SelectPort(id.source, id.node, port);
                linkStorage.InsertLinkAfter(i, id.source, id.node, sign);
                return;
            }

            int j = linkStorage.SelectPort(id.source, id.node, sign);
            linkStorage.InsertLinkAfter(j, id.source, id.node, port);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckHashLink(BlueprintMeta2 meta, IBlueprintSource source, NodeId id, NodeId runtimeId) {
            if (source is not IBlueprintHashLink hashLink) return;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            LinkValidator2.ValidateHashLink(hashLink, meta, id);
#endif

            hashLink.GetLinkedPort(id, out int hash, out int port);

            var portData = meta.GetPort(id, port);
            int hashRoot = _hashLinks.GetOrAddNode(hash);

            int dir = portData.IsInput() == portData.IsData() ? 0 : 1;
            int dirRoot = _hashLinks.GetOrAddNode(dir, hashRoot);

            _hashLinks.AddEndPoint(dirRoot, new RuntimeLink2(runtimeId.source, runtimeId.node, port));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckNodeLinks(
            IBlueprintFactory factory,
            BlueprintMeta2 meta,
            IRuntimeLinkStorage linkStorage,
            NodeId id,
            NodeId runtimeId,
            int port
        ) {
            int i = linkStorage.SelectPort(runtimeId.source, runtimeId.node, port);

            for (meta.TryGetLinksFrom(id, port, out int l); l >= 0; meta.TryGetNextLink(l, out l)) {
                var link = meta.GetLink(l);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                LinkValidator2.ValidateNodeLink(meta, id, port, link.id, link.port);
#endif

                var linkedId = GetOrCreateNode(factory, meta, link.id);
                i = linkStorage.InsertLinkAfter(i, linkedId.source, linkedId.node, link.port);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckExternalLink(
            BlueprintMeta2 meta,
            IRuntimeLinkStorage linkStorage,
            NodeId id,
            NodeId runtimeId,
            int port,
            NodeId rootId,
            int sign,
            bool isEnterOrOutput
        ) {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            LinkValidator2.ValidateExternalLink(meta, linkStorage, id, port, rootId, sign);
#endif

            // External port is enter or output port (link target):
            // Create link from matched subgraph root port to this external port.
            if (isEnterOrOutput) {
                int i = linkStorage.SelectPort(rootId.source, rootId.node, sign);
                linkStorage.InsertLinkAfter(i, runtimeId.source, runtimeId.node, port);
                return;
            }

            // External port is exit or input port (link owner):
            // Create link from this external port to matched subgraph root port.
            int j = linkStorage.SelectPort(runtimeId.source, runtimeId.node, port);
            linkStorage.InsertLinkAfter(j, rootId.source, rootId.node, sign);
            linkStorage.AddRootPort(sign);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckInternalLinks(
            BlueprintMeta2 meta,
            IBlueprintSource source,
            IRuntimeLinkStorage linkStorage,
            NodeId id,
            NodeId runtimeId,
            int port
        ) {
            if (source is not IBlueprintInternalLink internalLink) return;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            LinkValidator2.ValidateInternalLink(meta, id, port, internalLink);
#endif

            internalLink.GetLinkedPorts(id, port, out int s, out int count);
            int k = linkStorage.SelectPort(runtimeId.source, runtimeId.node, port);

            for (; s < count; s++) {
                k = linkStorage.InsertLinkAfter(k, runtimeId.source, runtimeId.node, s);
            }
        }
    }

}
