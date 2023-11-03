using System.Runtime.CompilerServices;
using MisterGames.Blueprints.Factory;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Nodes;
using MisterGames.Blueprints.Runtime;
using MisterGames.Blueprints.Validation;
using MisterGames.Common.Data;

namespace MisterGames.Blueprints.Compile {

    internal static class BlueprintCompilation {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NodeId CreateRootNode(IBlueprintFactory factory) {
            int sourceId = factory.GetOrCreateSource(typeof(BlueprintSourceRoot));
            int nodeId = factory.GetSource(sourceId).AddNode();
            return new NodeId(sourceId, nodeId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryCreateNode(
            IBlueprintFactory factory,
            BlueprintMeta2 meta,
            NodeId id,
            out NodeId runtimeId
        ) {
            var source = meta.GetNodeSource(id);

            if (source == null) {
                runtimeId = default;
                return false;
            }

            int runtimeSourceId = factory.GetOrCreateSource(source.GetType());
            var runtimeSource = factory.GetSource(runtimeSourceId);
            int runtimeNodeId = runtimeSource.AddNode();

            if (source is IBlueprintCloneable) {
                runtimeSource.SetNode(runtimeNodeId, source, id.node);
            }
            else {
                if (!meta.NodeJsonMap.TryGetValue(id, out string json)) {
                    json = source.GetNodeAsString(id.node);
                    meta.NodeJsonMap[id] = json;
                }

                runtimeSource.SetNode(runtimeNodeId, json);
            }

            runtimeId = new NodeId(runtimeSourceId, runtimeNodeId);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddHashLink(
            TreeMap<int, RuntimeLink2> links,
            BlueprintMeta2 meta,
            IBlueprintSource source,
            NodeId id,
            NodeId runtimeId
        ) {
            if (source is not IBlueprintHashLink hashLink) return;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            LinkValidator2.ValidateHashLink(hashLink, meta, id);
#endif

            hashLink.GetLinkedPort(id, out int hash, out int port);

            var portData = meta.GetPort(id, port);
            int hashRoot = links.GetOrAddNode(hash);

            int dir = portData.IsInput() == portData.IsData() ? 0 : 1;
            int dirRoot = links.GetOrAddNode(dir, hashRoot);

            links.AddEndPoint(dirRoot, new RuntimeLink2(runtimeId.source, runtimeId.node, port));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CompileHashLinks(
            IRuntimeLinkStorage linkStorage,
            TreeMap<int, RuntimeLink2> links
        ) {
            var hashes = links.Roots;

            foreach (int hash in hashes) {
                int hashRoot = links.GetNode(hash);
                int fromRoot = links.GetNode(0, hashRoot);
                int toRoot = links.GetNode(1, hashRoot);

                int t = links.GetChild(toRoot);

                for (int f = links.GetChild(fromRoot); f >= 0; f = links.GetNext(f)) {
                    var from = links.GetValueAt(f);
                    int i = linkStorage.SelectPort(from.source, from.node, from.port);

                    for (int l = t; l >= 0; l = links.GetNext(l)) {
                        var to = links.GetValueAt(l);
                        i = linkStorage.InsertLinkAfter(i, to.source, to.node, to.port);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CompileInternalLinks(
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

            for (int end = s + count; s < end; s++) {
                k = linkStorage.InsertLinkAfter(k, runtimeId.source, runtimeId.node, s);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CompileRootLinks(
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
            LinkValidator2.ValidateRootLink(meta, linkStorage, id, port, rootId, sign);
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
            linkStorage.AddOutRootPort(sign);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CompileSignatureLinks(
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
    }

}
