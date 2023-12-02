using System.Collections.Generic;
using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Meta;

namespace MisterGames.Blueprints.Editor.Utils {

    internal sealed class BlueprintMetaInvalidateTracker : IBlueprintMeta {

        public readonly HashSet<NodeId> nodesWithInvalidatedLinks = new HashSet<NodeId>();

        public Blackboard GetBlackboard() {
            return default;
        }

        public Port GetPort(NodeId id, int port) {
            return default;
        }

        public void AddPort(NodeId id, Port port) { }

        public int GetPortCount(NodeId id) {
            return 0;
        }

        public BlueprintLink GetLink(int index) {
            return default;
        }

        public bool TryGetLinksFrom(NodeId id, int port, out int index) {
            index = -1;
            return false;
        }

        public bool TryGetLinksTo(NodeId id, int port, out int index) {
            index = -1;
            return false;
        }

        public bool TryGetNextLink(int previous, out int next) {
            next = -1;
            return false;
        }

        public void SetSubgraph(NodeId id, BlueprintAsset asset) { }

        public void RemoveSubgraph(NodeId id) { }

        public bool InvalidateNode(NodeId id, bool invalidateLinks, bool notify = true) {
            if (invalidateLinks && notify) {
                nodesWithInvalidatedLinks.Add(id);
                return true;
            }

            return false;
        }
    }
}
