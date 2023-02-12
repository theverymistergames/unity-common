namespace MisterGames.Blueprints.Meta {

    /// <summary>
    /// Used by node to receive validation call with info about owner asset and node id
    /// to perform required validation.
    /// </summary>
    public interface IBlueprintAssetValidator {

        /// <summary>
        /// Called when node serialized data is being edited.
        /// </summary>
        void ValidateBlueprint(BlueprintAsset blueprint, int nodeId);
    }

}
