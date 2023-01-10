using MisterGames.Blueprints.Core2;

namespace MisterGames.Blueprints.Ports {

    public readonly struct RuntimeLink {

        public readonly BlueprintNode node;
        public readonly int port;

        public RuntimeLink(BlueprintNode node, int port) {
            this.node = node;
            this.port = port;
        }

        public override string ToString() {
            return $"RuntimeLink({node}::{port})";
        }
    }

}
