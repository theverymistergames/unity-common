namespace MisterGames.Blueprints.Core2 {

    public readonly struct RuntimeLink {

        public readonly BlueprintNode node;
        public readonly int port;

        public RuntimeLink(BlueprintNode node, int port) {
            this.node = node;
            this.port = port;
        }
    }

}
