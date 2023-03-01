namespace MisterGames.Blueprints.Meta {

    public interface IBlueprintPortDecorator {


        void DecoratePorts(BlueprintMeta blueprintMeta, int nodeId, Port[] ports);
    }

}
