using MisterGames.Blueprints.Factory;
using MisterGames.Blueprints.Runtime;

namespace MisterGames.Blueprints.Compile {

    public readonly struct BlueprintCompileData {

        public readonly IBlueprintHost2 host;
        public readonly IBlueprintFactory factory;
        public readonly IRuntimeNodeStorage nodeStorage;
        public readonly IRuntimeLinkStorage linkStorage;
        public readonly IRuntimeBlackboardStorage blackboardStorage;
        public readonly NodeId runtimeId;

        public BlueprintCompileData(
            IBlueprintHost2 host,
            IBlueprintFactory factory,
            IRuntimeNodeStorage nodeStorage,
            IRuntimeLinkStorage linkStorage,
            IRuntimeBlackboardStorage blackboardStorage,
            NodeId runtimeId
        ) {
            this.host = host;
            this.factory = factory;
            this.nodeStorage = nodeStorage;
            this.linkStorage = linkStorage;
            this.blackboardStorage = blackboardStorage;
            this.runtimeId = runtimeId;
        }
    }

}
