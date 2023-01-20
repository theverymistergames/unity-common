namespace MisterGames.Blueprints {

    /// <summary>
    /// Blueprint node has to implement this interface if it has an enter port.
    /// </summary>
    public interface IBlueprintEnter {

        /// <summary>
        /// Called when linked node is trying to call exit port.
        /// </summary>
        void OnEnterPort(int port);
    }

}
