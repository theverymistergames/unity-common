using MisterGames.Blueprints.Meta;

namespace MisterGames.Blueprints.Compile {

    internal interface IBlueprintCompiledNode {
        void Compile(BlueprintNodeMeta nodeMeta);
    }

}
