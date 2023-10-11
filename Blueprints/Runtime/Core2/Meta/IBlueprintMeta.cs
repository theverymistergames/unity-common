namespace MisterGames.Blueprints.Core2 {

    public interface IBlueprintMeta {

        bool TryGetLinks(long id, int port, out int firstLink);

        bool TryGetNextLink(int previousLink, out int nextLink);

        Port GetPort(int link);

        void InvalidateNode(long id, bool invalidateLinks = false);
    }

}
