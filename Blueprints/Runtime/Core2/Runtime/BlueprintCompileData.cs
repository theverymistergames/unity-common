namespace MisterGames.Blueprints.Core2 {

    public readonly ref struct BlueprintCompileData {

        public readonly IBlueprintFactory factory;
        public readonly IRuntimeNodeStorage nodeStorage;
        public readonly IRuntimeLinkStorage linkStorage;

        public BlueprintCompileData(
            IBlueprintFactory factory,
            IRuntimeNodeStorage nodeStorage,
            IRuntimeLinkStorage linkStorage
        ) {
            this.factory = factory;
            this.nodeStorage = nodeStorage;
            this.linkStorage = linkStorage;
        }
    }

}
