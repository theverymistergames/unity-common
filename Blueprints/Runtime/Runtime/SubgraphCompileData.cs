namespace MisterGames.Blueprints.Runtime {

    public readonly struct SubgraphCompileData {

        public readonly IBlueprintHost host;
        public readonly RuntimeBlueprint blueprint;
        public readonly NodeId id;
        public readonly NodeId runtimeId;
        public readonly int parent;

        public SubgraphCompileData(
            IBlueprintHost host,
            RuntimeBlueprint blueprint,
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
