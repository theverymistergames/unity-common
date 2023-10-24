namespace MisterGames.Blueprints.Core2 {

    public interface IBlueprintInternalLink {

        void GetLinkedPorts(NodeId id, int port, out int index, out int count);
    }

}
