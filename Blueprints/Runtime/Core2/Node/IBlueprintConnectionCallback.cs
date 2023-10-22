namespace MisterGames.Blueprints.Core2 {

    public interface IBlueprintConnectionCallback {

        void OnLinksChanged(IBlueprintMeta meta, NodeId id, int port);
    }

}
