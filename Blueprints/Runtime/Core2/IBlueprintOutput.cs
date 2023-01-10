namespace MisterGames.Blueprints.Core2 {

    public interface IBlueprintOutput<out T> {
        T GetPortValue(int port);
    }

    internal interface IBlueprintOutput {
        T GetPortValue<T>(int port);
    }

}
