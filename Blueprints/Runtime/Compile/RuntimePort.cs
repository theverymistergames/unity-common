namespace MisterGames.Blueprints.Compile {

    internal readonly struct RuntimePort {

        public readonly RuntimeLink[] links;

        public RuntimePort(RuntimeLink[] links) {
            this.links = links;
        }
    }

}
