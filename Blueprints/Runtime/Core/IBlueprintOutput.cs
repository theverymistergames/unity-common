namespace MisterGames.Blueprints {

    /// <summary>
    /// Blueprint node has to implement this interface if it has an output of type T port.
    /// </summary>
    public interface IBlueprintOutput<out T> {

        /// <summary>
        /// Called when linked node is trying to read value of type T.
        /// </summary>
        T GetOutputPortValue(int port);
    }

}
