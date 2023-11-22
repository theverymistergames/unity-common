using System.Collections.Generic;
using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Meta;

namespace MisterGames.Blueprints.Editor.View {

    internal class BlueprintNodePortCache : IBlueprintMeta {

        private readonly List<Port> _ports = new List<Port>();

        public void Clear() => _ports.Clear();

        public Blackboard GetBlackboard() => null;

        public Port GetPort(NodeId id, int port) => _ports[port];

        public void AddPort(NodeId id, Port port) => _ports.Add(port);

        public int GetPortCount(NodeId id) => _ports.Count;

        public BlueprintLink2 GetLink(int index) => default;

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

        public void SetSubgraph(NodeId id, BlueprintAsset2 asset) { }

        public void RemoveSubgraph(NodeId id) { }

        public bool InvalidateNode(NodeId id, bool invalidateLinks, bool notify = true) => false;
    }
}
