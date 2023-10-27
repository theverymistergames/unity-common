using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MisterGames.Common.Data;

namespace MisterGames.Blueprints.Core2 {

    public sealed class RuntimeLinkStorage : IRuntimeLinkStorage {

        public NodeId Root { get; set; }
        public HashSet<int> RootPorts { get; }

        private readonly TreeSet<RuntimeLink2> _linkTree;
        private RuntimeLink2 _selectedPort;

        private RuntimeLinkStorage() { }

        public RuntimeLinkStorage(int linkedPorts = 0, int links = 0) {
            RootPorts = new HashSet<int>();
            _linkTree = new TreeSet<RuntimeLink2>(linkedPorts, links);
            _linkTree.AllowDefragmentation(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetFirstLink(int source, int node, int port) {
            var address = new RuntimeLink2(source, node, port);
            return _linkTree.TryGetNode(address, out int portRoot) ? _linkTree.GetChild(portRoot) : -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetNextLink(int previous) {
            return _linkTree.GetNext(previous);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RuntimeLink2 GetLink(int index) {
            return _linkTree.GetKeyAt(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRootPort(int sign) {
            RootPorts.Add(sign);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SelectPort(int source, int node, int port) {
            _selectedPort = new RuntimeLink2(source, node, port);
            return GetFirstLink(source, node, port);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int InsertLinkAfter(int index, int source, int node, int port) {
            int portRoot = _linkTree.GetOrAddNode(_selectedPort);
            return _linkTree.InsertNextNode(new RuntimeLink2(source, node, port), portRoot, index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveLink(int source, int node, int port) {
            if (!_linkTree.TryGetNode(_selectedPort, out int portRoot)) return;

            _linkTree.RemoveNode(new RuntimeLink2(source, node, port), portRoot);
            if (!_linkTree.ContainsChildren(portRoot)) _linkTree.RemoveNodeAt(portRoot);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InlineLinks() {
            var roots = _linkTree.Roots;
            foreach (var root in roots) {
                int rootIndex = _linkTree.GetNode(root);
                int l = _linkTree.GetChild(rootIndex);

                while (l >= 0) {
                    var link = _linkTree.GetKeyAt(l);
                    int prev = _linkTree.GetPrevious(l);
                    int next = _linkTree.GetNext(l);

                    int s = _linkTree.TryGetNode(new RuntimeLink2(link.source, link.node, link.port), out int linkedPort)
                        ? _linkTree.GetChild(linkedPort)
                        : -1;

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
                    _linkTree.RemoveNodeAt(l);
                    l = prev;

                    for (; s >= 0; s = _linkTree.GetNext(s)) {
                        link = _linkTree.GetKeyAt(s);
                        l = _linkTree.InsertNextNode(link, rootIndex, l);

                        if (inlined) continue;

                        next = l;
                        inlined = true;
                    }

                    _linkTree.RemoveNodeAt(linkedPort);
                    l = next;
                }
            }
        }
    }

}
