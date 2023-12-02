namespace MisterGames.Blueprints.Nodes {

    /// <summary>
    /// Blueprint node callback, called when runtime blueprint starts
    /// at MonoBehaviour.Start of <see cref="BlueprintRunner"/> with corresponding <see cref="BlueprintAsset"/>.
    /// </summary>
    public interface IBlueprintStartCallback {

        /// <summary>
        /// Called when BlueprintRunner receives MonoBehaviour.Start call.
        /// </summary>
        void OnStart(IBlueprint blueprint, NodeToken token);
    }

}
