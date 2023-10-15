namespace MisterGames.Blueprints.Core2 {

    internal interface IBlueprintNodeLinker2 {

        void GetLinkedNode(long id, out int hash, out int port);
    }

}
