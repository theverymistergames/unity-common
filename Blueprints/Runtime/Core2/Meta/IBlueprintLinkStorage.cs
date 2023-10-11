namespace MisterGames.Blueprints.Core2 {

    public interface IBlueprintLinkStorage {

        ref BlueprintLink2 GetLink(int link);

        bool TryGetLinks(long id, int port, out int firstLink);

        bool TryGetNextLink(int previousLink, out int nextLink);

        void AddLink(long id, int port, long toId, int toPort);

        void RemoveLink(long id, int port, long toId, int toPort);

        void RemovePort(long id, int port);

        void RemoveNode(long id);
    }

}
