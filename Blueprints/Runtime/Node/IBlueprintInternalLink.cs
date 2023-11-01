namespace MisterGames.Blueprints.Nodes {

    public interface IBlueprintInternalLink {

        void GetLinkedPorts(NodeId id, int port, out int index, out int count);
    }

}
