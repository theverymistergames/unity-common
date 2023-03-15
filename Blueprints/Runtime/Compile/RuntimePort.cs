using System.Collections.Generic;

namespace MisterGames.Blueprints.Compile {

    public readonly struct RuntimePort {

        public readonly List<RuntimeLink> links;

        public RuntimePort(List<RuntimeLink> links) {
            this.links = links;
        }
        
        public void Call() {
            for (int i = 0; i < links.Count; i++) links[i].Call();
        }

        public R Get<R>(R defaultValue = default) {
            return links.Count > 0 ? links[0].Get(defaultValue) : defaultValue;
        }
    }

}
