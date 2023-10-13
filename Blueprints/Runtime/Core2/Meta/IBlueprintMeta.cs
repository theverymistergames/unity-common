namespace MisterGames.Blueprints.Core2 {

    public interface IBlueprintMeta {

        void AddPort(long id, int index, Port port);

        Port GetLinkedPort(int link);

        bool TryGetLinksFrom(long id, int port, out int firstLink);

        bool TryGetLinksTo(long id, int port, out int firstLink);

        bool TryGetNextLink(int previousLink, out int nextLink);

        void SetSubgraph(long id, BlueprintAsset2 asset);

        void RemoveSubgraph(long id);

        void InvalidateNode(long id, bool invalidatePorts = false);
    }

}
