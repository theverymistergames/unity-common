using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// Storage for blueprint links. It is used only in editor.
    /// </summary>
    [Serializable]
    public sealed class BlueprintLinkStorage : IBlueprintLinkStorage {

        [SerializeField] private TreeMap<int, BlueprintLink2> _linkTree;

        public ref BlueprintLink2 GetLink(int link) {
            return ref _linkTree.GetValueByRef(link);
        }

        public bool TryGetLinks(long id, int port, out int firstLink) {
            BlueprintNodeAddress.Unpack(id, out int factoryId, out int nodeId);
            firstLink = -1;

            if (!_linkTree.TryGetIndex(factoryId, out int factoryRoot) ||
                !_linkTree.TryGetIndex(nodeId, factoryRoot, out int nodeRoot) ||
                !_linkTree.TryGetIndex(port, nodeRoot, out int portRoot)
            ) {
                return false;
            }

            return _linkTree.TryGetChildIndex(portRoot, out firstLink);
        }

        public bool TryGetNextLink(int previousLink, out int nextLink) {
            return _linkTree.TryGetNextIndex(previousLink, out nextLink);
        }

        public void AddLink(long id, int port, long toId, int toPort) {
            BlueprintNodeAddress.Unpack(id, out int factoryId, out int nodeId);

            int factoryRoot = _linkTree.GetOrAddNode(factoryId);
            int nodeRoot = _linkTree.GetOrAddNode(nodeId, factoryRoot);
            int portRoot = _linkTree.GetOrAddNode(port, nodeRoot);

            _linkTree.AddEndPoint(portRoot, new BlueprintLink2 { nodeId = toId, port = toPort });
        }

        public void RemoveLink(long id, int port, long toId, int toPort) {
            BlueprintNodeAddress.Unpack(id, out int factoryId, out int nodeId);

            if (!_linkTree.TryGetIndex(factoryId, out int factoryRoot) ||
                !_linkTree.TryGetIndex(nodeId, factoryRoot, out int nodeRoot) ||
                !_linkTree.TryGetIndex(port, nodeRoot, out int portRoot) ||
                !_linkTree.TryGetChildIndex(portRoot, out int index)
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
                return;
            }
        }

        public void RemovePort(long id, int port) {
            BlueprintNodeAddress.Unpack(id, out int factoryId, out int nodeId);

            if (!_linkTree.TryGetIndex(factoryId, out int factoryRoot) ||
                !_linkTree.TryGetIndex(nodeId, factoryRoot, out int nodeRoot) ||
                !_linkTree.TryGetIndex(port, nodeRoot, out int portRoot)
            ) {
                return;
            }

            _linkTree.RemoveNodeAt(portRoot);
        }

        public void RemoveNode(long id) {
            BlueprintNodeAddress.Unpack(id, out int factoryId, out int nodeId);

            if (!_linkTree.TryGetIndex(factoryId, out int factoryRoot) ||
                !_linkTree.TryGetIndex(nodeId, factoryRoot, out int nodeRoot)
            ) {
                return;
            }

            _linkTree.RemoveNodeAt(nodeRoot);
        }

        public bool ContainsLink(long id, int port, long toId, int toPort) {
            BlueprintNodeAddress.Unpack(id, out int factoryId, out int nodeId);

            if (!_linkTree.TryGetIndex(factoryId, out int factoryRoot) ||
                !_linkTree.TryGetIndex(nodeId, factoryRoot, out int nodeRoot) ||
                !_linkTree.TryGetIndex(port, nodeRoot, out int portRoot) ||
                !_linkTree.TryGetChildIndex(portRoot, out int index)
            ) {
                return false;
            }

            while (index >= 0) {
                ref var link = ref _linkTree.GetValueByRefAt(index);

                if (link.nodeId != toId || link.port != toPort) {
                    index = _linkTree.GetNextIndex(index);
                    continue;
                }

                return true;
            }

            return false;
        }

        public bool ContainsPort(long id, int port) {
            BlueprintNodeAddress.Unpack(id, out int factoryId, out int nodeId);

            return _linkTree.TryGetIndex(factoryId, out int factoryRoot) &&
                   _linkTree.TryGetIndex(nodeId, factoryRoot, out int nodeRoot) &&
                   _linkTree.TryGetIndex(port, nodeRoot, out _);
        }

        public bool ContainsNode(long id) {
            BlueprintNodeAddress.Unpack(id, out int factoryId, out int nodeId);

            return _linkTree.TryGetIndex(factoryId, out int factoryRoot) &&
                   _linkTree.TryGetIndex(nodeId, factoryRoot, out _);
        }
    }

}
