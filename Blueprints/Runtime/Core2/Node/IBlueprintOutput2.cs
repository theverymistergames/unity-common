namespace MisterGames.Blueprints.Core2 {

    public interface IBlueprintOutput2 {
        T GetOutputPortValue<T>(IBlueprint blueprint, long id, int port);
    }

    public interface IBlueprintOutput2<out T> {
        T GetOutputPortValue(IBlueprint blueprint, long id, int port);
    }

}
