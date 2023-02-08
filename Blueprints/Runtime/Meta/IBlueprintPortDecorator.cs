#if UNITY_EDITOR

namespace MisterGames.Blueprints.Meta {

    internal interface IBlueprintPortDecorator {


        void DecoratePorts(BlueprintMeta blueprintMeta, int nodeId, Port[] ports);
    }

}

#endif
