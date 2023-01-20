namespace MisterGames.Blueprints.Validation {

    /// <summary>
    /// Used by node to receive validation call with info about owner asset and node id
    /// to perform required validation.
    /// </summary>
    internal interface IBlueprintValidatedNode {

        /// <summary>
        /// Called when node serialized data is being edited.
        /// </summary>
        void OnValidate(int nodeId, BlueprintAsset ownerAsset);
    }

}
