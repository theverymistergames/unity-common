using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Meta;

namespace MisterGames.Blueprints {

    public interface IBlueprintMeta {

        Blackboard GetBlackboard();

        Port GetPort(NodeId id, int port);

        void AddPort(NodeId id, Port port);

        int GetPortCount(NodeId id);

        BlueprintLink2 GetLink(int index);

        bool TryGetLinksFrom(NodeId id, int port, out int index);

        bool TryGetLinksTo(NodeId id, int port, out int index);

        bool TryGetNextLink(int previous, out int next);

        void SetSubgraph(NodeId id, BlueprintAsset2 asset);

        void RemoveSubgraph(NodeId id);

        bool InvalidateNode(NodeId id, bool invalidateLinks, bool notify = true);
    }

}
