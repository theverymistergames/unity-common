namespace MisterGames.Blueprints.Ports {

    public readonly struct RuntimePort {

        public readonly RuntimeLink[] links;

        public RuntimePort(RuntimeLink[] links) {
            this.links = links;
        }

        public override string ToString() {
            return $"RuntimePort(links: {string.Join(", ", links)})";
        }
    }

}
