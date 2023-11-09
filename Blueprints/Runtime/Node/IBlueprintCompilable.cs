using MisterGames.Blueprints.Runtime;

namespace MisterGames.Blueprints.Nodes {

    internal interface IBlueprintCompilable {

        void Compile(NodeId id, BlueprintCompileData data);
    }

}
