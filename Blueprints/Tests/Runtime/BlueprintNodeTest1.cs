using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core2;

namespace Core {

    [Serializable]
    public sealed class BlueprintFactoryTest1 : BlueprintFactory<BlueprintNodeTest1.Data> {
        public override BlueprintNode2 CreateNode() => new BlueprintNodeTest1();
    }

    [Serializable]
    public sealed class BlueprintNodeTest1 : BlueprintNode2 {

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
