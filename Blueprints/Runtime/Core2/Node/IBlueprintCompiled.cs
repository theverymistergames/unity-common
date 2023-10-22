namespace MisterGames.Blueprints.Core2 {

    internal interface IBlueprintCompiled {

        void OnCompile(IBlueprintMeta meta, NodeId id, BlueprintCompileData data);
    }

}
