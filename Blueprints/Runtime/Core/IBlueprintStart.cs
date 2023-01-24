namespace MisterGames.Blueprints {

    /// <summary>
    /// Used by Start built-in blueprint node to provide start call from Blueprint runner.
    /// </summary>
    internal interface IBlueprintStart {

        /// <summary>
        /// Called when BlueprintRunner receives MonoBehaviour.Start call.
        /// </summary>
        void OnStart();
    }

}
