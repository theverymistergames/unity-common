namespace MisterGames.Blueprints.Core2 {

    public interface IBlueprintConnectionsCallback {

        void OnConnectionsChanged(IBlueprintMeta meta, long id, int port);
    }

}
