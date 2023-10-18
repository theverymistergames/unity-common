namespace MisterGames.Blueprints.Core2 {

    public interface IBlueprintConnectionCallback {

        void OnLinksChanged(IBlueprintMeta meta, long id, int port);
    }

}
