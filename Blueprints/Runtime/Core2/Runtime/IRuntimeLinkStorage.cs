namespace MisterGames.Blueprints.Core2 {

    public interface IRuntimeLinkStorage {

        int GetPortCount(int source, int node);

        void SetPortCount(int source, int node, int count);

        int SelectPort(int source, int node, int port);

        void RemovePort(int source, int node, int port);

        int InsertLinkAfter(int index, int source, int node, int port);

        int RemoveLink(int index);

        int GetFirstLink(int source, int node, int port);

        int GetNextLink(int previous);

        RuntimeLink2 GetLink(int index);
    }

}
