namespace MisterGames.Blueprints.Runtime {

    internal readonly struct ExternalBlueprintData {

        public readonly NodeId caller;
        public readonly IBlueprint blueprint;

        public ExternalBlueprintData(NodeId caller, IBlueprint blueprint) {
            this.caller = caller;
            this.blueprint = blueprint;
        }
    }

}
