using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MisterGames.Common.Data;

namespace MisterGames.Blueprints.Core2 {

    internal static class BlueprintCompilation {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FetchExternalPorts(IBlueprintMeta meta, NodeId id, BlueprintAsset2 asset) {
            if (asset == null) return;

            var subgraphMeta = asset.BlueprintMeta;
            var nodes = subgraphMeta.Nodes;
            var portSignatureSet = new HashSet<int>();

            foreach (var subgraphNodeId in nodes) {
                int portCount = subgraphMeta.GetPortCount(subgraphNodeId);

                for (int p = 0; p < portCount; p++) {
                    var port = subgraphMeta.GetPort(subgraphNodeId, p);
                    if (!port.IsExternal()) continue;

                    int sign = port.GetSignature();

                    if (portSignatureSet.Contains(sign)) {
                        PortValidator2.ValidateExternalPortWithExistingSignature(subgraphMeta, port);
                        continue;
                    }

                    portSignatureSet.Add(sign);
                    meta.AddPort(id, port.External(false).Hide(false));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NodeId CreateRootNode(IBlueprintFactory factory) {
            int sourceId = factory.GetOrCreateSource(typeof(BlueprintSourceRoot));
            int nodeId = factory.GetSource(sourceId).AddNode();

            return new NodeId(sourceId, nodeId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NodeId CreateNode(
            IBlueprintFactory factory,
            IBlueprintSource source,
            NodeId id
        ) {
            int runtimeSourceId = factory.GetOrCreateSource(source.GetType());
            var runtimeSource = factory.GetSource(runtimeSourceId);

            int runtimeNodeId = runtimeSource.AddNodeCopy(source, id.node);
            return new NodeId(runtimeSourceId, runtimeNodeId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CompileHashLinks(IRuntimeLinkStorage linkStorage, TreeMap<int, RuntimeLink2> links) {
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
    }

}
