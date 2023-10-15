namespace MisterGames.Blueprints.Core2 {

    public interface IBlueprintConnectionsCallback {

        void OnLinksChanged(IBlueprintMeta meta, long id, int port);
    }

}
