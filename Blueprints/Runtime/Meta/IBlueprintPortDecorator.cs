namespace MisterGames.Blueprints.Meta {

    internal interface IBlueprintPortDecorator {


        void DecoratePorts(BlueprintMeta blueprintMeta, int nodeId, Port[] ports);
    }

}
