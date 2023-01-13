namespace MisterGames.Blueprints.Core2 {

    internal interface IBlueprintValidatedNode {
        void OnValidate(int nodeId, BlueprintAsset owner);
    }
}
