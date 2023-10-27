namespace MisterGames.Blueprints.Core2 {

    public interface IRuntimeNodeStorage {

        int Count { get; }

        NodeId GetNode(int index);

        void AddNode(NodeId id);

        void AllocateNodes(int count);
    }

}
