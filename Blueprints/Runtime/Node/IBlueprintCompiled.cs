using MisterGames.Blueprints.Compile;

namespace MisterGames.Blueprints.Nodes {

    internal interface IBlueprintCompiled {

        void Compile(NodeId id, BlueprintCompileData data);
    }

}
