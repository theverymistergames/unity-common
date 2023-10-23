using System.Collections.Generic;
using MisterGames.Common.Data;

namespace MisterGames.Blueprints.Core2 {

    public sealed class RuntimeLinkStorage : IRuntimeLinkStorage {

        private readonly TreeSet<RuntimeLink2> _linkTree;
        private readonly Dictionary<NodeId, int> _nodeToPortCountMap;

        private RuntimeLink2 _selectedPort = new RuntimeLink2(-1, -1, -1);

        public RuntimeLinkStorage() {
            _nodeToPortCountMap = new Dictionary<NodeId, int>();
            _linkTree = new TreeSet<RuntimeLink2>();
            _linkTree.AllowDefragmentation(false);
        }

        public RuntimeLinkStorage(int nodes, int linkedPorts, int links) {
            _nodeToPortCountMap = new Dictionary<NodeId, int>(nodes);
            _linkTree = new TreeSet<RuntimeLink2>(linkedPorts, links);
            _linkTree.AllowDefragmentation(false);
        }

        public int GetPortCount(int source, int node) {
            return _nodeToPortCountMap.TryGetValue(new NodeId(source, node), out int portCount) ? portCount : 0;
        }

        public void SetPortCount(int source, int node, int count) {
            _nodeToPortCountMap[new NodeId(source, node)] = count;
        }

        public int SelectPort(int source, int node, int port) {
            _selectedPort = new RuntimeLink2(source, node, port);
            return _linkTree.TryGetNode(_selectedPort, out int root) ? _linkTree.GetChild(root) : -1;
        }

        public void RemovePort(int source, int node, int port) {
            if (_selectedPort.source == source &&
                _selectedPort.node == node &&
                _selectedPort.port == port
            ) {
                _selectedPort = new RuntimeLink2(-1, -1, -1);
            }

            _linkTree.RemoveNode(new RuntimeLink2(source, node, port));
        }

        public int InsertLinkAfter(int index, int source, int node, int port) {
            if (_selectedPort.port < 0) return -1;

            var link = new RuntimeLink2(source, node, port);
            int portRoot = _linkTree.GetOrAddNode(_selectedPort);

            return _linkTree.InsertNextNode(link, portRoot, index);
        }

        public int RemoveLink(int index) {
            int previous = _linkTree.GetPrevious(index);
            _linkTree.RemoveNodeAt(index);

            return previous;
        }

        public int GetFirstLink(int source, int node, int port) {
            var key = new RuntimeLink2(source, node, port);
            return _linkTree.TryGetNode(key, out int index) ? _linkTree.GetChild(index) : -1;
        }

        public int GetNextLink(int previous) {
            return _linkTree.GetNext(previous);
        }

        public RuntimeLink2 GetLink(int index) {
            return _linkTree.GetKeyAt(index);
        }
    }

}
