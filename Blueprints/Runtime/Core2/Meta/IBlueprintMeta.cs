namespace MisterGames.Blueprints.Core2 {

    public interface IBlueprintMeta {

        Port GetPort(long id, int port);

        void AddPort(long id, Port port);

        int GetPortCount(long id);

        BlueprintLink2 GetLink(int index);

        bool TryGetLinksFrom(long id, int port, out int index);

        bool TryGetLinksTo(long id, int port, out int index);

        bool TryGetNextLink(int previous, out int next);

        void SetSubgraph(long id, BlueprintAsset2 asset);

        void RemoveSubgraph(long id);

        void InvalidateNode(long id, bool invalidateLinks, bool notify = true);
    }

}
