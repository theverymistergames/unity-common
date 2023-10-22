namespace MisterGames.Blueprints.Core2 {

    internal interface IBlueprintPortLinker2 {

        void GetLinkedPorts(NodeId id, int port, out int index, out int count);
    }

}
