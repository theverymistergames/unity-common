namespace MisterGames.Blueprints {

    public interface IBlueprintOutput {
        T GetPortValue<T>(IBlueprint blueprint, NodeToken token, int port);
    }

    public interface IBlueprintOutput<out T> {
        T GetPortValue(IBlueprint blueprint, NodeToken token, int port);
    }

}
