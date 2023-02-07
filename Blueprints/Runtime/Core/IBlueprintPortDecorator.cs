using MisterGames.Blueprints.Meta;

namespace MisterGames.Blueprints.Runtime.Core {

    internal interface IBlueprintPortDecorator {
        void DecoratePorts(BlueprintMeta blueprintMeta, int nodeId, Port[] ports);
    }
}
