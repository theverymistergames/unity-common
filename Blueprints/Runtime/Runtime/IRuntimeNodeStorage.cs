namespace MisterGames.Blueprints.Runtime {

    public interface IRuntimeNodeStorage {

        int Count { get; }

        NodeToken GetToken(int index);

        NodeId GetNode(int index);

        void AddNode(NodeId id, NodeId root);

        void AllocateNodes(int count);

        void Clear();
    }

}
