namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// Used by Start built-in blueprint node to provide start call from Blueprint runner.
    /// </summary>
    public interface IBlueprintStart2 {

        /// <summary>
        /// Called when BlueprintRunner receives MonoBehaviour.Start call.
        /// </summary>
        void OnStart(IBlueprint blueprint, long id);
    }

}
