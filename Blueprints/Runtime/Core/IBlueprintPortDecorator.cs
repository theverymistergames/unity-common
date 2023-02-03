namespace MisterGames.Blueprints.Runtime.Core {

    internal interface IBlueprintPortDecorator {
        void DecoratePorts(BlueprintAsset blueprint, int nodeId, Port[] ports);
    }
}
