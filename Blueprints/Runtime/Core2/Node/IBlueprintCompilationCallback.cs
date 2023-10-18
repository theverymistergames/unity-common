namespace MisterGames.Blueprints.Core2 {

    internal interface IBlueprintCompilationCallback {

        void OnCompile(long id, Port[] ports);
    }

}
