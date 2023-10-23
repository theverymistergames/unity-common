namespace MisterGames.Blueprints.Core2 {

    internal interface IBlueprintInternalLink {

        void GetLinkedPorts(NodeId id, int port, out int index, out int count);
    }

}
