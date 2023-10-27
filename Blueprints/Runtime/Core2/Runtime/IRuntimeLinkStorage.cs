using System.Collections.Generic;

namespace MisterGames.Blueprints.Core2 {

    public interface IRuntimeLinkStorage {

        NodeId Root { get; set; }

        HashSet<int> RootPorts { get; }

        void AddRootPort(int sign);

        int SelectPort(int source, int node, int port);

        int InsertLinkAfter(int index, int source, int node, int port);

        void RemoveLink(int source, int node, int port);

        int GetFirstLink(int source, int node, int port);

        int GetNextLink(int previous);

        RuntimeLink2 GetLink(int index);

        void InlineLinks();
    }

}
