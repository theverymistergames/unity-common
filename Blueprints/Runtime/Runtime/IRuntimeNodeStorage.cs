namespace MisterGames.Blueprints.Runtime {

    public interface IRuntimeNodeStorage {

        int Count { get; }

        NodeId GetNode(int index);

        void AddNode(NodeId id);
    }

}
