namespace MisterGames.Blueprints.Nodes {

    public interface IBlueprintHashLink {

        void GetLinkedPort(NodeId id, out int hash, out int port);
    }

}
