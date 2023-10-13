using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// Storage for blueprint links. It is used only in editor.
    /// </summary>
    [Serializable]
    public sealed class BlueprintLinkStorage {

        [SerializeField] private TreeMap<int, BlueprintLink2> _linkTree;

        public Action<long, int> OnPortChanged;

        public BlueprintLinkStorage(int capacity = 0) {
            _linkTree = new TreeMap<int, BlueprintLink2>(capacity);
        }

        public ref BlueprintLink2 GetLink(int link) {
            return ref _linkTree.GetValueByRefAt(link);
        }

        public bool TryGetLinksFrom(long id, int port, out int firstLink) {
            BlueprintNodeAddress.Unpack(id, out int sourceId, out int nodeId);
            firstLink = -1;

            if (!_linkTree.TryGetIndex(sourceId, out int sourceRoot) ||
                !_linkTree.TryGetIndex(nodeId, sourceRoot, out int nodeRoot) ||
                !_linkTree.TryGetIndex(port, nodeRoot, out int portRoot) ||
                !_linkTree.TryGetIndex(0, portRoot, out int linksRoot)
            ) {
                return false;
            }

            return _linkTree.TryGetChildIndex(linksRoot, out firstLink);
        }

        public bool TryGetLinksTo(long id, int port, out int firstLink) {
            BlueprintNodeAddress.Unpack(id, out int sourceId, out int nodeId);
            firstLink = -1;

            if (!_linkTree.TryGetIndex(sourceId, out int sourceRoot) ||
                !_linkTree.TryGetIndex(nodeId, sourceRoot, out int nodeRoot) ||
                !_linkTree.TryGetIndex(port, nodeRoot, out int portRoot) ||
                !_linkTree.TryGetIndex(1, portRoot, out int linksRoot)
            ) {
                return false;
            }

            return _linkTree.TryGetChildIndex(linksRoot, out firstLink);
        }

        public bool TryGetNextLink(int previousLink, out int nextLink) {
            return _linkTree.TryGetNextIndex(previousLink, out nextLink);
        }

        public void AddLink(long id, int port, long toId, int toPort) {
            AddLink(id, port, toId, toPort, 0);
            AddLink(toId, toPort, id, port, 1);

            OnPortChanged?.Invoke(id, port);
            OnPortChanged?.Invoke(toId, toPort);
        }

        public void RemoveLink(long id, int port, long toId, int toPort) {
            _linkTree.AllowDefragmentation(false);

            RemoveLink(id, port, toId, toPort, 0);
            RemoveLink(toId, toPort, id, port, 1);

            _linkTree.AllowDefragmentation(true);
        }

        public void RemovePort(long id, int port) {
            BlueprintNodeAddress.Unpack(id, out int sourceId, out int nodeId);

            if (!_linkTree.TryGetIndex(sourceId, out int sourceRoot) ||
                !_linkTree.TryGetIndex(nodeId, sourceRoot, out int nodeRoot) ||
                !_linkTree.TryGetIndex(port, nodeRoot, out int portRoot)
            ) {
                return;
            }

            _linkTree.AllowDefragmentation(false);

            RemovePortLinks(portRoot, id, port);

            _linkTree.RemoveNodeAt(portRoot);
            if (!_linkTree.HasChildren(nodeRoot)) _linkTree.RemoveNodeAt(nodeRoot);
            if (!_linkTree.HasChildren(sourceRoot)) _linkTree.RemoveNodeAt(sourceRoot);

            _linkTree.AllowDefragmentation(true);

            OnPortChanged?.Invoke(id, port);
        }

        public void RemoveNode(long id) {
            BlueprintNodeAddress.Unpack(id, out int sourceId, out int nodeId);

            if (!_linkTree.TryGetIndex(sourceId, out int sourceRoot) ||
                !_linkTree.TryGetIndex(nodeId, sourceRoot, out int nodeRoot)
            ) {
                return;
            }

            _linkTree.AllowDefragmentation(false);

            int portRoot = _linkTree.GetChildIndex(nodeRoot);
            while (portRoot >= 0) {
                int port = _linkTree.GetKeyAt(portRoot);

                RemovePortLinks(portRoot, id, port);
                portRoot = _linkTree.GetNextIndex(portRoot);

                OnPortChanged?.Invoke(id, port);
            }

            _linkTree.RemoveNodeAt(nodeRoot);
            if (!_linkTree.HasChildren(sourceRoot)) _linkTree.RemoveNodeAt(sourceRoot);

            _linkTree.AllowDefragmentation(true);
        }

        public bool ContainsLink(long id, int port, long toId, int toPort) {
            BlueprintNodeAddress.Unpack(id, out int sourceId, out int nodeId);

            if (!_linkTree.TryGetIndex(sourceId, out int sourceRoot) ||
                !_linkTree.TryGetIndex(nodeId, sourceRoot, out int nodeRoot) ||
                !_linkTree.TryGetIndex(port, nodeRoot, out int portRoot) ||
                !_linkTree.TryGetIndex(0, portRoot, out int linksRoot) ||
                !_linkTree.TryGetChildIndex(linksRoot, out int index)
            ) {
                return false;
            }

            while (index >= 0) {
                ref var link = ref _linkTree.GetValueByRefAt(index);
                if (link.nodeId == toId && link.port == toPort) return true;

                index = _linkTree.GetNextIndex(index);
            }

            return false;
        }

        public bool ContainsPort(long id, int port) {
            BlueprintNodeAddress.Unpack(id, out int sourceId, out int nodeId);

            return _linkTree.TryGetIndex(sourceId, out int sourceRoot) &&
                   _linkTree.TryGetIndex(nodeId, sourceRoot, out int nodeRoot) &&
                   _linkTree.TryGetIndex(port, nodeRoot, out _);
        }

        public bool ContainsNode(long id) {
            BlueprintNodeAddress.Unpack(id, out int sourceId, out int nodeId);

            return _linkTree.TryGetIndex(sourceId, out int sourceRoot) &&
                   _linkTree.TryGetIndex(nodeId, sourceRoot, out _);
        }

        private void AddLink(long id, int port, long toId, int toPort, int dir) {
            BlueprintNodeAddress.Unpack(id, out int sourceId, out int nodeId);

            int sourceRoot = _linkTree.GetOrAddNode(sourceId);
            int nodeRoot = _linkTree.GetOrAddNode(nodeId, sourceRoot);
            int portRoot = _linkTree.GetOrAddNode(port, nodeRoot);
            int linksRoot = _linkTree.GetOrAddNode(dir, portRoot);

            _linkTree.AddEndPoint(linksRoot, new BlueprintLink2 { nodeId = toId, port = toPort });
        }

        private void RemoveLink(long id, int port, long toId, int toPort, int dir) {
            BlueprintNodeAddress.Unpack(id, out int sourceId, out int nodeId);

            if (!_linkTree.TryGetIndex(sourceId, out int sourceRoot) ||
                !_linkTree.TryGetIndex(nodeId, sourceRoot, out int nodeRoot) ||
                !_linkTree.TryGetIndex(port, nodeRoot, out int portRoot) ||
                !_linkTree.TryGetIndex(dir, portRoot, out int linksRoot) ||
                !_linkTree.TryGetChildIndex(linksRoot, out int index)
            ) {
                return;
            }

            while (index >= 0) {
                ref var link = ref _linkTree.GetValueByRefAt(index);

                if (link.nodeId != toId || link.port != toPort) {
                    index = _linkTree.GetNextIndex(index);
                    continue;
                }

                _linkTree.RemoveNodeAt(index);

                if (!_linkTree.HasChildren(linksRoot)) _linkTree.RemoveNodeAt(linksRoot);
                if (!_linkTree.HasChildren(portRoot)) _linkTree.RemoveNodeAt(portRoot);
                if (!_linkTree.HasChildren(nodeRoot)) _linkTree.RemoveNodeAt(nodeRoot);
                if (!_linkTree.HasChildren(sourceRoot)) _linkTree.RemoveNodeAt(sourceRoot);

                OnPortChanged?.Invoke(id, port);

                return;
            }
        }

        private void RemovePortLinks(int portRoot, long id, int port) {
            if (_linkTree.TryGetIndex(0, portRoot, out int linksRoot) &&
                _linkTree.TryGetChildIndex(linksRoot, out int index)
            ) {
                while (index >= 0) {
                    ref var link = ref _linkTree.GetValueByRefAt(index);
                    RemoveLink(link.nodeId, link.port, id, port, 1);

                    index = _linkTree.GetNextIndex(index);
                }
            }

            if (_linkTree.TryGetIndex(1, portRoot, out linksRoot) &&
                _linkTree.TryGetChildIndex(linksRoot, out index)
            ) {
                while (index >= 0) {
                    ref var link = ref _linkTree.GetValueByRefAt(index);
                    RemoveLink(link.nodeId, link.port, id, port, 0);

                    index = _linkTree.GetNextIndex(index);
                }
            }
        }

        public void Clear() {
            _linkTree.Clear();
            OnPortChanged = null;
        }
    }

}
