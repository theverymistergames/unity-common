namespace MisterGames.Blueprints.Nodes {

    internal interface IBlueprintHashLink {

        void GetLinkedPort(NodeId id, out int hash, out int port);
    }

}
