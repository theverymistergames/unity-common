namespace MisterGames.Blueprints.Meta {

    public interface IBlueprintPortDecorator {


        void DecoratePorts(BlueprintAsset blueprint, int nodeId, Port[] ports);
    }

}
