namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// Blueprint node has to implement this interface if it has an output of type T port.
    /// </summary>
    /// <typeparam name="T">Port data type</typeparam>
    public interface IBlueprintOutput<out T> {
        T GetPortValue(int port);
    }

    /// <summary>
    /// Used by Input/Output built-in blueprint nodes:
    /// those nodes don`t have information about port data type at compile time.
    /// </summary>
    internal interface IBlueprintOutput {
        T GetPortValue<T>(int port);
    }

}
