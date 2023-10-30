namespace MisterGames.Blueprints.Nodes {

    public interface IBlueprintConnectionCallback {

        void OnLinksChanged(IBlueprintMeta meta, NodeId id, int port);
    }

}
