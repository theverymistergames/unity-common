namespace MisterGames.Blueprints.Runtime {

    public readonly struct SubgraphCompileData {

        public readonly IBlueprintHost2 host;
        public readonly RuntimeBlueprint2 blueprint;
        public readonly NodeId id;
        public readonly NodeId runtimeId;
        public readonly int parent;

        public SubgraphCompileData(
            IBlueprintHost2 host,
            RuntimeBlueprint2 blueprint,
            NodeId id,
            NodeId runtimeId,
            int parent
        ) {
            this.host = host;
            this.blueprint = blueprint;
            this.id = id;
            this.runtimeId = runtimeId;
            this.parent = parent;
        }
    }

}
