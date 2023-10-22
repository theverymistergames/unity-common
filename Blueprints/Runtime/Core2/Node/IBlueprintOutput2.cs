namespace MisterGames.Blueprints.Core2 {

    public interface IBlueprintOutput2 {
        T GetPortValue<T>(IBlueprint blueprint, NodeId id, int port);
    }

    public interface IBlueprintOutput2<out T> {
        T GetPortValue(IBlueprint blueprint, NodeId id, int port);
    }

}
