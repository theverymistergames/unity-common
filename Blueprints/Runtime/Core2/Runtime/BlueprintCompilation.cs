using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MisterGames.Common.Data;

namespace MisterGames.Blueprints.Core2 {

    internal static class BlueprintCompilation {

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
        public static void CompileInternalLink(
            IBlueprintInternalLink link,
            IRuntimeLinkStorage linkStorage,
            NodeId id,
            NodeId runtimeId,
            int port
        ) {
            link.GetLinkedPorts(id, port, out int s, out int count);
            int k = linkStorage.SelectPort(runtimeId.source, runtimeId.node, port);

            for (; s < count; s++) {
                k = linkStorage.InsertLinkAfter(k, runtimeId.source, runtimeId.node, s);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CompileExternalLinks(
            IRuntimeLinkStorage linkStorage,
            RuntimeLink2 root,
            NodeId id,
            int port,
            bool isEnterOrOutput
        ) {
            // External port is enter or output port (link target):
            // Create link from matched subgraph root port to this external port.
            if (isEnterOrOutput) {
                int k = linkStorage.SelectPort(root.source, root.node, root.port);
                linkStorage.InsertLinkAfter(k, id.source, id.node, port);
            }
            // External port is exit or input port (link owner):
            // Create link from this external port to matched subgraph root port.
            else {
                int k = linkStorage.SelectPort(id.source, id.node, port);
                linkStorage.InsertLinkAfter(k, root.source, root.node, root.port);
            }
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddHashLink(
            TreeMap<int, RuntimeLink2> links,
            IBlueprintMeta meta,
            NodeId id,
            NodeId runtimeId,
            IBlueprintHashLink link
        ) {
            link.GetLinkedPort(id, out int hash, out int port);

            var portData = meta.GetPort(id, port);
            int hashRoot = links.GetOrAddNode(hash);

            int dir = portData.IsInput() == portData.IsData() ? 0 : 1;
            int dirRoot = links.GetOrAddNode(dir, hashRoot);

            links.AddEndPoint(dirRoot, new RuntimeLink2(runtimeId.source, runtimeId.node, port));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FillSignatureToPortMap(
            Dictionary<int, RuntimeLink2> map,
            IBlueprintMeta meta,
            NodeId id,
            NodeId runtimeId
        ) {
            int count = meta.GetPortCount(id);

            for (int p = 0; p < count; p++) {
                var port = meta.GetPort(id, p);
                var address = new RuntimeLink2(runtimeId.source, runtimeId.node, p);

                map.Add(port.GetSignature(), address);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InlineLinks(IRuntimeNodeStorage nodeStorage, IRuntimeLinkStorage linkStorage) {
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
