namespace MisterGames.Blueprints.Core {

    /// <summary>
    /// Used by Start built-in blueprint node to provide start call from Blueprint runner.
    /// </summary>
    public interface IBlueprintStart {

        /// <summary>
        /// Called when BlueprintRunner receives MonoBehaviour.Start call.
        /// </summary>
        void OnStart();
    }

}
