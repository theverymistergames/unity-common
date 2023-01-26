using System.Collections.Generic;

namespace MisterGames.Blueprints.Compile {

    internal readonly struct RuntimePort {

        public readonly List<RuntimeLink> links;

        public RuntimePort(List<RuntimeLink> links) {
            this.links = links;
        }
    }

}
