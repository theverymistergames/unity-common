using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core2;

namespace Core {

    [Serializable]
    public sealed class BlueprintFactoryTest0 : BlueprintFactory<BlueprintNodeTest0.Data> {
        public override BlueprintNode2 CreateNode() => new BlueprintNodeTest0();
    }

    [Serializable]
    public sealed class BlueprintNodeTest0 : BlueprintNode2 {

        [Serializable]
        public struct Data {
            public int intValue;
            public float floatValue;
        }

        public override Port[] CreatePorts(IBlueprint blueprint, long id) {
            return Array.Empty<Port>();
        }
    }

}
