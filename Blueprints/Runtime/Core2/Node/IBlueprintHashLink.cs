namespace MisterGames.Blueprints.Core2 {

    public interface IBlueprintHashLink {

        void GetLinkedPort(NodeId id, out int hash, out int port);
    }

}
