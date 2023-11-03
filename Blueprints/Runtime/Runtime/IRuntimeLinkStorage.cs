namespace MisterGames.Blueprints.Runtime {

    public interface IRuntimeLinkStorage {

        int SelectPort(int source, int node, int port);

        int InsertLinkAfter(int index, int source, int node, int port);

        void RemoveLink(int source, int node, int port);

        int GetFirstLink(int source, int node, int port);

        int GetNextLink(int previous);

        RuntimeLink2 GetLink(int index);

        void InlineLinks();
    }

}
