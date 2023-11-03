using System.Collections.Generic;
using MisterGames.Common.Data;

namespace MisterGames.Blueprints.Runtime {

    public sealed class RuntimeLinkStorage : IRuntimeLinkStorage {

        public NodeId Root { get; set; }
        public HashSet<int> OutRootPorts { get; } = new HashSet<int>();

        private readonly TreeSet<RuntimeLink2> _linkTree;
        private RuntimeLink2 _selectedPort;

        private RuntimeLinkStorage() { }

        public RuntimeLinkStorage(int linkedPorts = 0, int links = 0) {
            _linkTree = new TreeSet<RuntimeLink2>(linkedPorts, links);
            _linkTree.AllowDefragmentation(false);
        }

        public void AddOutRootPort(int sign) {
            OutRootPorts.Add(sign);
        }

        public int GetFirstLink(int source, int node, int port) {
            var address = new RuntimeLink2(source, node, port);
            return _linkTree.TryGetNode(address, out int portRoot) ? _linkTree.GetChild(portRoot) : -1;
        }

        public int GetNextLink(int previous) {
            return _linkTree.GetNext(previous);
        }

        public RuntimeLink2 GetLink(int index) {
            return _linkTree.GetKeyAt(index);
        }

        public int SelectPort(int source, int node, int port) {
            _selectedPort = new RuntimeLink2(source, node, port);
            return GetFirstLink(source, node, port);
        }

        public int InsertLinkAfter(int index, int source, int node, int port) {
            int portRoot = _linkTree.GetOrAddNode(_selectedPort);
            return _linkTree.InsertNextNode(new RuntimeLink2(source, node, port), portRoot, index);
        }

        public void RemoveLink(int source, int node, int port) {
            if (!_linkTree.TryGetNode(_selectedPort, out int portRoot)) return;

            _linkTree.RemoveNode(new RuntimeLink2(source, node, port), portRoot);
            if (!_linkTree.ContainsChildren(portRoot)) _linkTree.RemoveNodeAt(portRoot);
        }

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

                    while (s >= 0) {
                        link = _linkTree.GetKeyAt(s);
                        l = _linkTree.InsertNextNode(link, rootIndex, l);

                        if (!inlined) {
                            next = l;
                            inlined = true;
                        }

                        int n = _linkTree.GetNext(s);
                        _linkTree.RemoveNodeAt(s);
                        s = n;
                    }

                    l = next;
                }
            }
        }

        public override string ToString() {
            return $"{nameof(RuntimeLinkStorage)}: links: {_linkTree}";
        }
    }

}
