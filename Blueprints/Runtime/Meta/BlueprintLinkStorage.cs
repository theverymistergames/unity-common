using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Meta {

    /// <summary>
    /// Storage for blueprint links. It is used only in editor.
    /// </summary>
    [Serializable]
    public sealed class BlueprintLinkStorage {

        [SerializeField] private TreeSet<BlueprintLink> _linkTree;
        [SerializeField] private int _linkCount;
        [SerializeField] private int _linkedPortCount;

        public Action<NodeId, int> OnPortChanged { get; set; }

        public int LinkCount => _linkCount;
        public int LinkedPortCount => _linkedPortCount;

        public BlueprintLinkStorage() {
            _linkTree = new TreeSet<BlueprintLink>();
        }

        public BlueprintLinkStorage(BlueprintLinkStorage source) {
            _linkTree = new TreeSet<BlueprintLink>(source._linkTree);
            _linkCount = source._linkCount;
            _linkedPortCount = source._linkedPortCount;
        }

        public BlueprintLink GetLink(int link) {
            return _linkTree.GetKeyAt(link);
        }

        public bool TryGetLinksFrom(NodeId id, int port, out int index) {
            if (!_linkTree.TryGetNode(new BlueprintLink(id, 0), out int nodeRoot) ||
                !_linkTree.TryGetNode(new BlueprintLink(id, port), nodeRoot, out int portRoot) ||
                !_linkTree.TryGetNode(new BlueprintLink(id, 0), portRoot, out int linksRoot)
            ) {
                index = -1;
                return false;
            }

            return _linkTree.TryGetChild(linksRoot, out index);
        }

        public bool TryGetLinksTo(NodeId id, int port, out int index) {
            if (!_linkTree.TryGetNode(new BlueprintLink(id, 0), out int nodeRoot) ||
                !_linkTree.TryGetNode(new BlueprintLink(id, port), nodeRoot, out int portRoot) ||
                !_linkTree.TryGetNode(new BlueprintLink(id, 1), portRoot, out int linksRoot)
            ) {
                index = -1;
                return false;
            }

            return _linkTree.TryGetChild(linksRoot, out index);
        }

        public bool TryGetNextLink(int previous, out int next) {
            return _linkTree.TryGetNext(previous, out next);
        }

        public void SortLinksFrom(NodeId id, int port, IComparer<BlueprintLink> comparer) {
            if (!_linkTree.TryGetNode(new BlueprintLink(id, 0), out int nodeRoot) ||
                !_linkTree.TryGetNode(new BlueprintLink(id, port), nodeRoot, out int portRoot) ||
                !_linkTree.TryGetNode(new BlueprintLink(id, 0), portRoot, out int linksRoot)
            ) {
                return;
            }

            _linkTree.SortChildren(linksRoot, comparer);
        }

        public TreeSet<BlueprintLink> CopyLinks(NodeId id) {
            return _linkTree.TryGetNode(new BlueprintLink(id, 0), out int nodeRoot)
                ? _linkTree.Copy(nodeRoot, includeRoot: false)
                : null;
        }

        public void SetLinks(NodeId id, int port, TreeSet<BlueprintLink> links, int sourcePort, bool notify = true) {
            if (links == null || !links.TryGetNode(new BlueprintLink(id, sourcePort), out int portRoot)) return;

            if (links.TryGetNode(new BlueprintLink(id, 0), portRoot, out int linksRoot) &&
                links.TryGetChild(linksRoot, out int l)
            ) {
                while (l >= 0) {
                    var link = links.GetKeyAt(l);
                    AddLink(id, port, link.id, link.port, notify);
                    links.TryGetNext(l, out l);
                }
            }

            if (links.TryGetNode(new BlueprintLink(id, 1), portRoot, out linksRoot) &&
                links.TryGetChild(linksRoot, out l)
            ) {
                while (l >= 0) {
                    var link = links.GetKeyAt(l);
                    AddLink(link.id, link.port, id, port, notify);
                    links.TryGetNext(l, out l);
                }
            }
        }

        public void AddLink(NodeId id, int port, NodeId toId, int toPort, bool notify = true) {
            AddLink(id, port, toId, toPort, 0);
            AddLink(toId, toPort, id, port, 1);

            _linkCount++;

            if (notify) {
                OnPortChanged?.Invoke(id, port);
                OnPortChanged?.Invoke(toId, toPort);
            }
        }

        public bool RemoveLink(NodeId id, int port, NodeId toId, int toPort, bool notify = true) {
            _linkTree.AllowDefragmentation(false);

            bool removed = RemoveLink(id, port, toId, toPort, 0, notify) |
                           RemoveLink(toId, toPort, id, port, 1, notify) |
                           RemoveLink(toId, toPort, id, port, 0, notify) |
                           RemoveLink(id, port, toId, toPort, 1, notify);

            if (removed) {
                _linkCount--;
                if (_linkCount < 0) _linkCount = 0;
            }

            _linkTree.AllowDefragmentation(true);

            return removed;
        }

        public void RemovePort(NodeId id, int port, bool notify = true) {
            if (!_linkTree.TryGetNode(new BlueprintLink(id, 0), out int nodeRoot) ||
                !_linkTree.TryGetNode(new BlueprintLink(id, port), nodeRoot, out int portRoot)
            ) {
                return;
            }

            _linkTree.AllowDefragmentation(false);

            RemovePortLinks(portRoot, id, port, notify);

            _linkTree.RemoveNodeAt(portRoot);
            if (!_linkTree.ContainsChildren(nodeRoot)) _linkTree.RemoveNodeAt(nodeRoot);

            _linkTree.AllowDefragmentation(true);

            if (notify) OnPortChanged?.Invoke(id, port);
        }

        public void RemoveNode(NodeId id, bool notify = true) {
            if (!_linkTree.TryGetNode(new BlueprintLink(id, 0), out int nodeRoot)) return;

            _linkTree.AllowDefragmentation(false);

            int portRoot = _linkTree.GetChild(nodeRoot);
            while (portRoot >= 0) {
                int port = _linkTree.GetKeyAt(portRoot).port;

                RemovePortLinks(portRoot, id, port, notify);
                if (notify) OnPortChanged?.Invoke(id, port);

                portRoot = _linkTree.GetNext(portRoot);
            }

            _linkTree.RemoveNodeAt(nodeRoot);

            _linkTree.AllowDefragmentation(true);
        }

        public bool ContainsLink(NodeId id, int port, NodeId toId, int toPort) {
            return _linkTree.TryGetNode(new BlueprintLink(id, 0), out int nodeRoot) &&
                   _linkTree.TryGetNode(new BlueprintLink(id, port), nodeRoot, out int portRoot) &&
                   _linkTree.TryGetNode(new BlueprintLink(id, 0), portRoot, out int linksRoot) &&
                   _linkTree.ContainsNode(new BlueprintLink(toId, toPort), linksRoot);
        }

        public bool ContainsPort(NodeId id, int port) {
            return _linkTree.TryGetNode(new BlueprintLink(id, 0), out int nodeRoot) &&
                   _linkTree.ContainsNode(new BlueprintLink(id, port), nodeRoot);
        }

        public bool ContainsNode(NodeId id) {
            return _linkTree.ContainsNode(new BlueprintLink(id, 0));
        }

        public void Clear() {
            _linkTree.Clear();
            _linkCount = 0;
            _linkedPortCount = 0;
        }

        private void AddLink(NodeId id, int port, NodeId toId, int toPort, int dir) {
            int nodeRoot = _linkTree.GetOrAddNode(new BlueprintLink(id, 0));
            int portRoot = _linkTree.GetOrAddNode(new BlueprintLink(id, port), nodeRoot);

            var dirKey = new BlueprintLink(id, dir);
            if (dir == 0 && !_linkTree.ContainsNode(dirKey, portRoot)) _linkedPortCount++;

            int linksRoot = _linkTree.GetOrAddNode(dirKey, portRoot);

            _linkTree.GetOrAddNode(new BlueprintLink(toId, toPort), linksRoot);
        }

        private bool RemoveLink(NodeId id, int port, NodeId toId, int toPort, int dir, bool notify) {
            if (!_linkTree.TryGetNode(new BlueprintLink(id, 0), out int nodeRoot) ||
                !_linkTree.TryGetNode(new BlueprintLink(id, port), nodeRoot, out int portRoot) ||
                !_linkTree.TryGetNode(new BlueprintLink(id, dir), portRoot, out int linksRoot) ||
                !_linkTree.TryGetNode(new BlueprintLink(toId, toPort), linksRoot, out int link)
            ) {
                return false;
            }

            _linkTree.RemoveNodeAt(link);

            if (!_linkTree.ContainsChildren(linksRoot)) {
                _linkTree.RemoveNodeAt(linksRoot);
                if (dir == 0) _linkedPortCount--;
                if (_linkedPortCount < 0) _linkedPortCount = 0;
            }

            if (!_linkTree.ContainsChildren(portRoot)) _linkTree.RemoveNodeAt(portRoot);
            if (!_linkTree.ContainsChildren(nodeRoot)) _linkTree.RemoveNodeAt(nodeRoot);

            if (notify) OnPortChanged?.Invoke(id, port);
            return true;
        }

        private void RemovePortLinks(int portRoot, NodeId id, int port, bool notify) {
            if (_linkTree.TryGetNode(new BlueprintLink(id, 0), portRoot, out int linksRoot) &&
                _linkTree.TryGetChild(linksRoot, out int index)
            ) {
                while (index >= 0) {
                    var link = _linkTree.GetKeyAt(index);

                    if (RemoveLink(link.id, link.port, id, port, 1, notify)) _linkCount--;

                    index = _linkTree.GetNext(index);
                }

                _linkedPortCount--;
            }

            if (_linkTree.TryGetNode(new BlueprintLink(id, 1), portRoot, out linksRoot) &&
                _linkTree.TryGetChild(linksRoot, out index)
            ) {
                while (index >= 0) {
                    var link = _linkTree.GetKeyAt(index);

                    if (RemoveLink(link.id, link.port, id, port, 0, notify)) _linkCount--;

                    index = _linkTree.GetNext(index);
                }
            }

            if (_linkedPortCount < 0) _linkedPortCount = 0;
            if (_linkCount < 0) _linkCount = 0;
        }

        public override string ToString() {
            return $"{nameof(BlueprintLinkStorage)}: links {_linkTree}";
        }
    }

}
