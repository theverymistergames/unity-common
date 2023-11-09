namespace MisterGames.Blueprints.Runtime {

    public readonly struct BlueprintCompileData {

        public readonly IBlueprintHost2 host;
        public readonly RuntimeBlueprint2 blueprint;
        public readonly NodeId runtimeId;

        public BlueprintCompileData(
            IBlueprintHost2 host,
            RuntimeBlueprint2 blueprint,
            NodeId runtimeId
        ) {
            this.host = host;
            this.blueprint = blueprint;
            this.runtimeId = runtimeId;
        }
    }

}
