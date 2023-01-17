namespace MisterGames.Blueprints.Core2 {

    public readonly struct RuntimePort {

        public readonly RuntimeLink[] links;

        public RuntimePort(RuntimeLink[] links) {
            this.links = links;
        }
    }

}
