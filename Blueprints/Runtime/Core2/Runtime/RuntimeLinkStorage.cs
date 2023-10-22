using System;
using System.Collections.Generic;
using MisterGames.Common.Data;

namespace MisterGames.Blueprints.Core2 {

    public sealed class RuntimeLinkStorage : IRuntimeLinkStorage {

        private readonly TreeMap<RuntimeLink2, RuntimeLink2> _links;
        private readonly Dictionary<NodeId, int> _nodes;

        private RuntimeLink2 _selectedPort = new RuntimeLink2(-1, -1, -1);

        public RuntimeLinkStorage(int nodes, int linkedPorts, int links) {
            _nodes = new Dictionary<NodeId, int>(nodes);
            _links = new TreeMap<RuntimeLink2, RuntimeLink2>(linkedPorts, links);
            _links.AllowDefragmentation(false);
        }

        public int GetPortCount(int source, int node) {
            return _nodes.TryGetValue(new NodeId(source, node), out int portCount) ? portCount : 0;
        }

        public void SetPortCount(int source, int node, int count) {
            _nodes[new NodeId(source, node)] = count;
        }

        public int SelectPort(int source, int node, int port) {
            _selectedPort = new RuntimeLink2(source, node, port);
            return _links.TryGetNode(_selectedPort, out int root) ? _links.GetChild(root) : -1;
        }

        public void RemovePort(int source, int node, int port) {
            if (_selectedPort.source == source &&
                _selectedPort.node == node &&
                _selectedPort.port == port
            ) {
                _selectedPort = new RuntimeLink2(-1, -1, -1);
            }

            _links.RemoveNode(new RuntimeLink2(source, node, port));
        }

        public int InsertLinkAfter(int index, int source, int node, int port) {
            if (_selectedPort.port < 0) {
                throw new InvalidOperationException($"{nameof(RuntimeLinkStorage)}: " +
                                                    $"cannot insert link before port is selected.");
            }

            var link = new RuntimeLink2(source, node, port);
            return index < 0
                ? _links.AddEndPoint(_links.GetOrAddNode(_selectedPort), link)
                : _links.InsertNextEndPoint(index, link);
        }

        public int RemoveLink(int index) {
            int previous = _links.GetPrevious(index);
            _links.RemoveNodeAt(index);

            return previous;
        }

        public int GetFirstLink(int source, int node, int port) {
            var key = new RuntimeLink2(source, node, port);
            return _links.TryGetNode(key, out int index) ? _links.GetChild(index) : -1;
        }

        public int GetNextLink(int previous) {
            return _links.GetNext(previous);
        }

        public RuntimeLink2 GetLink(int index) {
            return _links.GetValueAt(index);
        }
    }

}
