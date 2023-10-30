namespace MisterGames.Blueprints.Nodes {

    /// <summary>
    /// Blueprint node callback, called when runtime blueprint starts
    /// at MonoBehaviour.Start of <see cref="BlueprintRunner2"/> with corresponding <see cref="BlueprintAsset2"/>.
    /// </summary>
    public interface IBlueprintStartCallback {

        /// <summary>
        /// Called when BlueprintRunner receives MonoBehaviour.Start call.
        /// </summary>
        void OnStart(IBlueprint blueprint, NodeId id);
    }

}
