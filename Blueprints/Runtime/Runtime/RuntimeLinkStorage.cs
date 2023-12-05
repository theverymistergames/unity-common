using MisterGames.Common.Data;

namespace MisterGames.Blueprints.Runtime {

    public sealed class RuntimeLinkStorage : IRuntimeLinkStorage {

        private readonly TreeSet<RuntimeLink> _linkTree;
        private RuntimeLink _selectedPort;

        private RuntimeLinkStorage() { }

        public RuntimeLinkStorage(int linkedPorts = 0, int links = 0) {
            _linkTree = new TreeSet<RuntimeLink>(linkedPorts, links);
            _linkTree.AllowDefragmentation(false);
        }

        public int GetFirstLink(int source, int node, int port) {
            var address = new RuntimeLink(source, node, port);
            return _linkTree.TryGetNode(address, out int portRoot) ? _linkTree.GetChild(portRoot) : -1;
        }

        public int GetNextLink(int previous) {
            return _linkTree.GetNext(previous);
        }

        public RuntimeLink GetLink(int index) {
            return _linkTree.GetKeyAt(index);
        }

        public int SelectPort(int source, int node, int port) {
            _selectedPort = new RuntimeLink(source, node, port);
            return GetFirstLink(source, node, port);
        }

        public int InsertLinkAfter(int index, int source, int node, int port) {
            int portRoot = _linkTree.GetOrAddNode(_selectedPort);
            return _linkTree.InsertNextNode(new RuntimeLink(source, node, port), portRoot, index);
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

                    int s = _linkTree.TryGetNode(new RuntimeLink(link.source, link.node, link.port), out int linkedPort)
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
                    // 3) Return to the first inlined link to continue inline checks

                    // Remove original link
                    bool inlined = false;
                    _linkTree.RemoveNodeAt(l);
                    l = prev;

                    for (; s >= 0; s = _linkTree.GetNext(s)) {
                        // Add inlined link
                        link = _linkTree.GetKeyAt(s);
                        l = _linkTree.InsertNextNode(link, rootIndex, l);

                        // Found first link to inline, this is the next link to check for inline
                        if (!inlined) {
                            next = l;
                            inlined = true;
                        }
                    }

                    l = next;
                }
            }
        }

        public void Clear() {
            _linkTree.Clear();
            _selectedPort = default;
        }

        public override string ToString() {
            return $"{nameof(RuntimeLinkStorage)}: links: {_linkTree}";
        }
    }

}
