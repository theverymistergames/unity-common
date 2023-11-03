using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MisterGames.Blueprints.Factory;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Nodes;
using MisterGames.Blueprints.Runtime;
using MisterGames.Blueprints.Validation;
using MisterGames.Common.Data;

namespace MisterGames.Blueprints.Compile {

    internal sealed class BlueprintCompiler2 {

        private readonly Dictionary<NodeId, NodeId> _runtimeNodeMap = new Dictionary<NodeId, NodeId>();
        private readonly HashSet<NodeId> _compiledNodes = new HashSet<NodeId>();
        private readonly TreeMap<int, RuntimeLink2> _hashLinks = new TreeMap<int, RuntimeLink2>();

        public RuntimeBlueprint2 Compile(IBlueprintHost2 host, IBlueprintFactory factory, BlueprintAsset2 asset) {
            _runtimeNodeMap.Clear();
            _compiledNodes.Clear();
            _hashLinks.Clear();

            var root = BlueprintCompilation.CreateRootNode(factory);
            var meta = asset.BlueprintMeta;

            var nodeStorage = new RuntimeNodeStorage(meta.NodeCount + 1);
            var linkStorage = new RuntimeLinkStorage(meta.LinkedPortCount, meta.LinkCount);
            var blackboardStorage = new RuntimeBlackboardStorage(meta.SubgraphAssets.Count + 1);

            blackboardStorage.SetBlackboard(root, host.GetBlackboard(asset));

            CompileNodes(host, factory, meta, nodeStorage, linkStorage, blackboardStorage, root);
            BlueprintCompilation.CompileHashLinks(linkStorage, _hashLinks);

            linkStorage.InlineLinks();

            return new RuntimeBlueprint2(factory, nodeStorage, linkStorage, blackboardStorage);
        }

        public void CompileSubgraph(BlueprintAsset2 asset, BlueprintCompileData data) {
            _runtimeNodeMap.Clear();
            _compiledNodes.Clear();
            _hashLinks.Clear();

            var host = data.host;
            var root = data.runtimeId;
            var meta = asset.BlueprintMeta;

            var nodeStorage = data.nodeStorage;
            var linkStorage = data.linkStorage;
            var blackboardStorage = data.blackboardStorage;

            blackboardStorage.SetBlackboard(root, host.GetBlackboard(asset));
            nodeStorage.AllocateNodes(meta.NodeCount);

            CompileNodes(host, data.factory, meta, nodeStorage, linkStorage, blackboardStorage, root);
            BlueprintCompilation.CompileHashLinks(linkStorage, _hashLinks);
        }

        private void CompileNodes(
            IBlueprintHost2 host,
            IBlueprintFactory factory,
            BlueprintMeta2 meta,
            IRuntimeNodeStorage nodeStorage,
            IRuntimeLinkStorage linkStorage,
            IRuntimeBlackboardStorage blackboardStorage,
            NodeId rootId
        ) {
            linkStorage.Root = rootId;

            var nodes = meta.Nodes;
            foreach (var id in nodes) {
                if (TryGetOrCompileNode(host, factory, meta, nodeStorage, linkStorage, blackboardStorage, id, rootId, out var runtimeId)) {
                    nodeStorage.AddNode(runtimeId);
                }
            }
        }

        private bool TryGetOrCompileNode(
            IBlueprintHost2 host,
            IBlueprintFactory factory,
            BlueprintMeta2 meta,
            IRuntimeNodeStorage nodeStorage,
            IRuntimeLinkStorage linkStorage,
            IRuntimeBlackboardStorage blackboardStorage,
            NodeId id,
            NodeId rootId,
            out NodeId runtimeId
        ) {
            if (_compiledNodes.Contains(id)) {
                runtimeId = _runtimeNodeMap[id];
                return true;
            }

            if (!TryGetOrCreateNode(factory, meta, id, out runtimeId)) return false;

            var source = meta.GetNodeSource(id);
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
                    BlueprintCompilation.CompileRootLinks(meta, linkStorage, id, runtimeId, p, rootId, sign, isEnterOrOutput);
                }
                else {
                    BlueprintCompilation.CompileSignatureLinks(source, linkStorage, id, p, sign, isEnterOrOutput);
                }

                if (isEnterOrOutput) {
                    BlueprintCompilation.CompileInternalLinks(meta, source, linkStorage, id, runtimeId, p);
                    continue;
                }

                // External ports can not have links to other nodes.
                if (!isExternal) CompileNodeLinks(factory, meta, linkStorage, id, runtimeId, p);
            }

            BlueprintCompilation.AddHashLink(_hashLinks, meta, source, id, runtimeId);

            if (source is IBlueprintCompiled compiled) {
                compiled.Compile(id, new BlueprintCompileData(host, factory, nodeStorage, linkStorage, blackboardStorage, runtimeId));
            }

            _compiledNodes.Add(runtimeId);

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryGetOrCreateNode(IBlueprintFactory factory, BlueprintMeta2 meta, NodeId id, out NodeId runtimeId) {
            if (_runtimeNodeMap.TryGetValue(id, out runtimeId)) return true;
            if (!BlueprintCompilation.TryCreateNode(factory, meta, id, out runtimeId)) return false;

            _runtimeNodeMap[id] = runtimeId;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CompileNodeLinks(
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

                if (TryGetOrCreateNode(factory, meta, link.id, out var linkedId)) {
                    i = linkStorage.InsertLinkAfter(i, linkedId.source, linkedId.node, link.port);
                }
            }
        }
    }

}
