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

        public RuntimeBlueprint2 Compile(IBlueprintFactory factory, IBlueprintHost2 host, BlueprintAsset2 asset) {
            _runtimeNodeMap.Clear();
            _compiledNodes.Clear();
            _hashLinks.Clear();

            var root = BlueprintCompilation.CreateRootNode(factory);
            var meta = asset.BlueprintMeta;

            var nodeStorage = new RuntimeNodeStorage(meta.NodeCount + 1);
            var linkStorage = new RuntimeLinkStorage(meta.LinkedPortCount, meta.LinkCount);
            var blackboardStorage = new RuntimeBlackboardStorage(meta.SubgraphAssets.Count + 1);
            var blueprint = new RuntimeBlueprint2(root, factory, nodeStorage, linkStorage, blackboardStorage);

            blackboardStorage.SetBlackboard(root, host.GetBlackboard(asset));

            CompileNodes(host, meta, blueprint, root);
            BlueprintCompilation.CompileHashLinks(linkStorage, _hashLinks);

            linkStorage.InlineLinks();

            return blueprint;
        }

        public void CompileSubgraph(BlueprintAsset2 asset, BlueprintCompileData data) {
            _runtimeNodeMap.Clear();
            _compiledNodes.Clear();
            _hashLinks.Clear();

            var host = data.host;
            var root = data.runtimeId;
            var meta = asset.BlueprintMeta;

            data.blueprint.blackboardStorage.SetBlackboard(root, host.GetBlackboard(asset));
            data.blueprint.nodeStorage.AllocateNodes(meta.NodeCount);

            CompileNodes(host, meta, data.blueprint, root);
            BlueprintCompilation.CompileHashLinks(data.blueprint.linkStorage, _hashLinks);
        }

        private void CompileNodes(
            IBlueprintHost2 host,
            BlueprintMeta2 meta,
            RuntimeBlueprint2 blueprint,
            NodeId rootId
        ) {
            var nodes = meta.Nodes;
            var nodeStorage = blueprint.nodeStorage;

            foreach (var id in nodes) {
                if (TryGetOrCompileNode(host, meta, blueprint, id, rootId, out var runtimeId)) {
                    nodeStorage.AddNode(runtimeId);
                }
            }
        }

        private bool TryGetOrCompileNode(
            IBlueprintHost2 host,
            BlueprintMeta2 meta,
            RuntimeBlueprint2 blueprint,
            NodeId id,
            NodeId rootId,
            out NodeId runtimeId
        ) {
            if (_compiledNodes.Contains(id)) {
                runtimeId = _runtimeNodeMap[id];
                return true;
            }

            if (!TryGetOrCreateNode(blueprint.factory, meta, id, out runtimeId)) return false;

            var linkStorage = blueprint.linkStorage;
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
#if UNITY_EDITOR
                    blueprint.rootPorts[sign] = port;
#endif

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
                if (!isExternal) CompileNodeLinks(meta, blueprint, id, runtimeId, p);
            }

            BlueprintCompilation.AddHashLink(_hashLinks, meta, source, id, runtimeId);

            if (source is IBlueprintCompiled compiled) {
                compiled.Compile(id, new BlueprintCompileData(host, blueprint, runtimeId));
            }

            _compiledNodes.Add(runtimeId);

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryGetOrCreateNode(
            IBlueprintFactory factory,
            BlueprintMeta2 meta,
            NodeId id,
            out NodeId runtimeId
        ) {
            if (_runtimeNodeMap.TryGetValue(id, out runtimeId)) return true;
            if (!BlueprintCompilation.TryCreateNode(factory, meta, id, out runtimeId)) return false;

            _runtimeNodeMap[id] = runtimeId;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CompileNodeLinks(
            BlueprintMeta2 meta,
            RuntimeBlueprint2 blueprint,
            NodeId id,
            NodeId runtimeId,
            int port
        ) {
            var linkStorage = blueprint.linkStorage;
            var factory = blueprint.factory;

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
