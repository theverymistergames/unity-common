using MisterGames.Blueprints.Factory;
using MisterGames.Blueprints.Runtime;

namespace MisterGames.Blueprints.Compile {

    public readonly struct BlueprintCompileData {

        public readonly IBlueprintFactory factory;
        public readonly IRuntimeLinkStorage linkStorage;
        public readonly NodeId runtimeId;

        public BlueprintCompileData(
            IBlueprintFactory factory,
            IRuntimeLinkStorage linkStorage,
            NodeId runtimeId
        ) {
            this.factory = factory;
            this.linkStorage = linkStorage;
            this.runtimeId = runtimeId;
        }
    }

}
