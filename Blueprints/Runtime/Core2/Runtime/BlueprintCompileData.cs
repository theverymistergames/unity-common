namespace MisterGames.Blueprints.Core2 {

    public readonly struct BlueprintCompileData {

        public readonly IBlueprintMeta meta;
        public readonly NodeId id;
        public readonly NodeId runtimeId;
        public readonly IRuntimeNodeStorage nodeStorage;
        public readonly IRuntimeLinkStorage linkStorage;

        public BlueprintCompileData(
            IBlueprintMeta meta,
            NodeId id,
            NodeId runtimeId,
            IRuntimeNodeStorage nodeStorage,
            IRuntimeLinkStorage linkStorage
        ) {
            this.meta = meta;
            this.id = id;
            this.runtimeId = runtimeId;
            this.nodeStorage = nodeStorage;
            this.linkStorage = linkStorage;
        }
    }

}
