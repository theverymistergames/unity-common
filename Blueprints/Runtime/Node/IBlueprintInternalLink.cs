namespace MisterGames.Blueprints.Nodes {

    internal interface IBlueprintInternalLink {

        void GetLinkedPorts(NodeId id, int port, out int index, out int count);
    }

}
