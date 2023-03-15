namespace MisterGames.Blueprints {

    public interface IBlueprintOutput {
        R GetOutputPortValue<R>(int port);
    }

    public interface IBlueprintOutput<out R> {
        R GetOutputPortValue(int port);
    }

}
