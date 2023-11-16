namespace MisterGames.Blueprints {

    public interface IBlueprintOutput2 {
        T GetPortValue<T>(NodeToken token, int port);
    }

    public interface IBlueprintOutput2<out T> {
        T GetPortValue(NodeToken token, int port);
    }

}
