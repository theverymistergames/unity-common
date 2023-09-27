namespace MisterGames.Blueprints.Core2 {

    public interface IBlueprintOutput2 {
        T GetOutputPortValue<T>(int port, IBlueprint blueprint, long id);
    }

    public interface IBlueprintOutput2<out T> {
        T GetOutputPortValue(int port, IBlueprint blueprint, long id);
    }

}
