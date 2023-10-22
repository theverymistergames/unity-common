namespace MisterGames.Blueprints.Core2 {

    internal interface IBlueprintNodeLinker2 {

        void GetLinkedNode(NodeId id, out int hash, out int port);
    }

}
