using MisterGames.Blueprints.Compile;

namespace MisterGames.Blueprints.Nodes {

    internal interface IBlueprintCompilable {

        void Compile(NodeId id, BlueprintCompileData data);
    }

}
