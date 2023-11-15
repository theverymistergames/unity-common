namespace MisterGames.Blueprints.Nodes {

    public interface IBlueprintHashLink {

        bool TryGetLinkedPort(NodeId id, out int hash, out int port);
    }

}
