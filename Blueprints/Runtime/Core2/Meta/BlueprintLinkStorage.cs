using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// Storage for blueprint links. It is used only in editor.
    /// </summary>
    [Serializable]
    public sealed class BlueprintLinkStorage {

        [SerializeField] private TreeMap<int, BlueprintLink2> _linkTree;
        [SerializeField] private int _linkCount;
        [SerializeField] private int _linkedPortCount;

        public Action<NodeId, int> OnPortChanged { get; set; }

        public int LinkCount => _linkCount;
        public int LinkedPortCount => _linkedPortCount;

        public BlueprintLinkStorage(int capacity = 0) {
            _linkTree = new TreeMap<int, BlueprintLink2>(capacity);
        }

        public BlueprintLink2 GetLink(int link) {
            return _linkTree.GetValueAt(link);
        }

        public bool TryGetLinksFrom(NodeId id, int port, out int index) {
            if (!_linkTree.TryGetNode(id.source, out int sourceRoot) ||
                !_linkTree.TryGetNode(id.node, sourceRoot, out int nodeRoot) ||
                !_linkTree.TryGetNode(port, nodeRoot, out int portRoot) ||
                !_linkTree.TryGetNode(0, portRoot, out int linksRoot)
            ) {
                index = -1;
                return false;
            }

            return _linkTree.TryGetChild(linksRoot, out index);
        }

        public bool TryGetLinksTo(NodeId id, int port, out int index) {
            if (!_linkTree.TryGetNode(id.source, out int sourceRoot) ||
                !_linkTree.TryGetNode(id.node, sourceRoot, out int nodeRoot) ||
                !_linkTree.TryGetNode(port, nodeRoot, out int portRoot) ||
                !_linkTree.TryGetNode(1, portRoot, out int linksRoot)
            ) {
                index = -1;
                return false;
            }

            return _linkTree.TryGetChild(linksRoot, out index);
        }

        public bool TryGetNextLink(int previous, out int next) {
            return _linkTree.TryGetNext(previous, out next);
        }

        public void SortLinksFrom(NodeId id, int port, IComparer<BlueprintLink2> comparer) {
            if (!_linkTree.TryGetNode(id.source, out int sourceRoot) ||
                !_linkTree.TryGetNode(id.node, sourceRoot, out int nodeRoot) ||
                !_linkTree.TryGetNode(port, nodeRoot, out int portRoot) ||
                !_linkTree.TryGetNode(0, portRoot, out int linksRoot)
            ) {
                return;
            }

            _linkTree.SortChildren(linksRoot, comparer);
        }

        public TreeMap<int, BlueprintLink2> CopyLinks(NodeId id) {
            if (!_linkTree.TryGetNode(id.source, out int sourceRoot) ||
                !_linkTree.TryGetNode(id.node, sourceRoot, out int nodeRoot)
            ) {
                return null;
            }

            return _linkTree.Copy(nodeRoot, includeRoot: false);
        }

        public void SetLinks(NodeId id, int port, TreeMap<int, BlueprintLink2> links, int sourcePort) {
            if (!links.TryGetNode(sourcePort, out int root)) return;

            if (links.TryGetNode(0, root, out int linksRoot) &&
                links.TryGetChild(linksRoot, out int l)
            ) {
                while (l >= 0) {
                    var link = links.GetValueAt(l);
                    AddLink(id, port, link.id, link.port);

                    links.TryGetNext(l, out l);
                }
            }

            if (links.TryGetNode(1, root, out linksRoot) &&
                links.TryGetChild(linksRoot, out l)
            ) {
                while (l >= 0) {
                    var link = links.GetValueAt(l);
                    AddLink(link.id, link.port, id, port);

                    links.TryGetNext(l, out l);
                }
            }
        }

        public void AddLink(NodeId id, int port, NodeId toId, int toPort) {
            AddLink(id, port, toId, toPort, 0);
            AddLink(toId, toPort, id, port, 1);

            _linkCount++;

            OnPortChanged?.Invoke(id, port);
            OnPortChanged?.Invoke(toId, toPort);
        }

        public void RemoveLink(NodeId id, int port, NodeId toId, int toPort) {
            _linkTree.AllowDefragmentation(false);

            RemoveLink(id, port, toId, toPort, 0);
            RemoveLink(toId, toPort, id, port, 1);

            _linkCount--;

            _linkTree.AllowDefragmentation(true);
        }

        public void RemovePort(NodeId id, int port) {
            if (!_linkTree.TryGetNode(id.source, out int sourceRoot) ||
                !_linkTree.TryGetNode(id.node, sourceRoot, out int nodeRoot) ||
                !_linkTree.TryGetNode(port, nodeRoot, out int portRoot)
            ) {
                return;
            }

            _linkTree.AllowDefragmentation(false);

            RemovePortLinks(portRoot, id, port);

            _linkTree.RemoveNodeAt(portRoot);
            if (!_linkTree.ContainsChildren(nodeRoot)) _linkTree.RemoveNodeAt(nodeRoot);
            if (!_linkTree.ContainsChildren(sourceRoot)) _linkTree.RemoveNodeAt(sourceRoot);

            _linkTree.AllowDefragmentation(true);

            OnPortChanged?.Invoke(id, port);
        }

        public void RemoveNode(NodeId id) {
            if (!_linkTree.TryGetNode(id.source, out int sourceRoot) ||
                !_linkTree.TryGetNode(id.node, sourceRoot, out int nodeRoot)
            ) {
                return;
            }

            _linkTree.AllowDefragmentation(false);

            int portRoot = _linkTree.GetChild(nodeRoot);
            while (portRoot >= 0) {
                int port = _linkTree.GetKeyAt(portRoot);

                RemovePortLinks(portRoot, id, port);
                OnPortChanged?.Invoke(id, port);

                portRoot = _linkTree.GetNext(portRoot);
            }

            _linkTree.RemoveNodeAt(nodeRoot);
            if (!_linkTree.ContainsChildren(sourceRoot)) _linkTree.RemoveNodeAt(sourceRoot);

            _linkTree.AllowDefragmentation(true);
        }

        public bool ContainsLink(NodeId id, int port, NodeId toId, int toPort) {
            if (!_linkTree.TryGetNode(id.source, out int sourceRoot) ||
                !_linkTree.TryGetNode(id.node, sourceRoot, out int nodeRoot) ||
                !_linkTree.TryGetNode(port, nodeRoot, out int portRoot) ||
                !_linkTree.TryGetNode(0, portRoot, out int linksRoot) ||
                !_linkTree.TryGetChild(linksRoot, out int index)
            ) {
                return false;
            }

            while (index >= 0) {
                ref var link = ref _linkTree.GetValueAt(index);
                if (link.id == toId && link.port == toPort) return true;

                index = _linkTree.GetNext(index);
            }

            return false;
        }

        public bool ContainsPort(NodeId id, int port) {
            return _linkTree.TryGetNode(id.source, out int sourceRoot) &&
                   _linkTree.TryGetNode(id.node, sourceRoot, out int nodeRoot) &&
                   _linkTree.TryGetNode(port, nodeRoot, out _);
        }

        public bool ContainsNode(NodeId id) {
            return _linkTree.TryGetNode(id.source, out int sourceRoot) &&
                   _linkTree.TryGetNode(id.node, sourceRoot, out _);
        }

        public void Clear() {
            _linkTree.Clear();
            _linkCount = 0;
            _linkedPortCount = 0;
            OnPortChanged = null;
        }

        private void AddLink(NodeId id, int port, NodeId toId, int toPort, int dir) {
            int sourceRoot = _linkTree.GetOrAddNode(id.source);
            int nodeRoot = _linkTree.GetOrAddNode(id.node, sourceRoot);
            int portRoot = _linkTree.GetOrAddNode(port, nodeRoot);

            if (dir == 0 && !_linkTree.ContainsNode(dir, portRoot)) _linkedPortCount++;

            int linksRoot = _linkTree.GetOrAddNode(dir, portRoot);

            _linkTree.AddEndPoint(linksRoot, new BlueprintLink2(toId, toPort));
        }

        private void RemoveLink(NodeId id, int port, NodeId toId, int toPort, int dir) {
            if (!_linkTree.TryGetNode(id.source, out int sourceRoot) ||
                !_linkTree.TryGetNode(id.node, sourceRoot, out int nodeRoot) ||
                !_linkTree.TryGetNode(port, nodeRoot, out int portRoot) ||
                !_linkTree.TryGetNode(dir, portRoot, out int linksRoot) ||
                !_linkTree.TryGetChild(linksRoot, out int index)
            ) {
                return;
            }

            while (index >= 0) {
                ref var link = ref _linkTree.GetValueAt(index);

                if (link.id != toId || link.port != toPort) {
                    index = _linkTree.GetNext(index);
                    continue;
                }

                _linkTree.RemoveNodeAt(index);

                if (!_linkTree.ContainsChildren(linksRoot)) {
                    _linkTree.RemoveNodeAt(linksRoot);
                    if (dir == 0) _linkedPortCount--;
                }

                if (!_linkTree.ContainsChildren(portRoot)) _linkTree.RemoveNodeAt(portRoot);
                if (!_linkTree.ContainsChildren(nodeRoot)) _linkTree.RemoveNodeAt(nodeRoot);
                if (!_linkTree.ContainsChildren(sourceRoot)) _linkTree.RemoveNodeAt(sourceRoot);

                OnPortChanged?.Invoke(id, port);

                return;
            }
        }

        private void RemovePortLinks(int portRoot, NodeId id, int port) {
            if (_linkTree.TryGetNode(0, portRoot, out int linksRoot) &&
                _linkTree.TryGetChild(linksRoot, out int index)
            ) {
                while (index >= 0) {
                    ref var link = ref _linkTree.GetValueAt(index);

                    RemoveLink(link.id, link.port, id, port, 1);
                    _linkCount--;

                    index = _linkTree.GetNext(index);
                }

                _linkedPortCount--;
            }

            if (_linkTree.TryGetNode(1, portRoot, out linksRoot) &&
                _linkTree.TryGetChild(linksRoot, out index)
            ) {
                while (index >= 0) {
                    ref var link = ref _linkTree.GetValueAt(index);

                    RemoveLink(link.id, link.port, id, port, 0);
                    _linkCount--;

                    index = _linkTree.GetNext(index);
                }
            }
        }

        public override string ToString() {
            return $"{nameof(BlueprintLinkStorage)}: links {_linkTree}";
        }
    }

}
