namespace MisterGames.Blueprints {

    /// <summary>
    /// Blueprint node has to implement this interface if it has an output of type T port.
    /// </summary>
    public interface IBlueprintOutput<out T> {

        /// <summary>
        /// Called when linked node is trying to read value of type T.
        /// </summary>
        T GetPortValue(int port);
    }

    /// <summary>
    /// Used by Input/Output built-in blueprint nodes:
    /// those nodes don`t have information about port data type at compile time.
    /// </summary>
    internal interface IBlueprintOutput {

        /// <summary>
        /// Called when linked node is trying to read value of type T.
        /// </summary>
        T GetPortValue<T>(int port);
    }

}
