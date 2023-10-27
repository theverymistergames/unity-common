namespace MisterGames.Blueprints.Core2 {

    internal interface IBlueprintHashLink {

        void GetLinkedPort(NodeId id, out int hash, out int port);
    }

}
