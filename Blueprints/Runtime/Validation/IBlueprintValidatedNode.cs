namespace MisterGames.Blueprints.Validation {

    internal interface IBlueprintValidatedNode {
        void OnValidate(int nodeId, BlueprintAsset ownerAsset);
    }

}
