namespace MisterGames.Blueprints.Core2 {

    public readonly struct BlueprintCompileData {

        public readonly IBlueprintFactory factory;
        public readonly IRuntimeNodeStorage nodeStorage;
        public readonly IRuntimeLinkStorage linkStorage;
        public readonly NodeId runtimeId;

        public BlueprintCompileData(
            IBlueprintFactory factory,
            IRuntimeNodeStorage nodeStorage,
            IRuntimeLinkStorage linkStorage,
            NodeId runtimeId
        ) {
            this.factory = factory;
            this.nodeStorage = nodeStorage;
            this.linkStorage = linkStorage;
            this.runtimeId = runtimeId;
        }
    }

}
