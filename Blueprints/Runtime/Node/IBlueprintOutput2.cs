namespace MisterGames.Blueprints {

    public interface IBlueprintOutput2 {
        T GetPortValue<T>(IBlueprint blueprint, NodeToken token, int port);
    }

    public interface IBlueprintOutput2<out T> {
        T GetPortValue(IBlueprint blueprint, NodeToken token, int port);
    }

}
