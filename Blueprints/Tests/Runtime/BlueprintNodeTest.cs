using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core2;

namespace Core {

    [Serializable]
    public sealed class BlueprintNodeTestFactory : BlueprintFactory<BlueprintNodeTestData> {
        public override BlueprintNode2 CreateNode() => new BlueprintNodeTest();
    }

    [Serializable]
    public struct BlueprintNodeTestData {
        public int intValue;
        public float floatValue;
    }

    [Serializable]
    public sealed class BlueprintNodeTest : BlueprintNode2 {

        public override Port[] CreatePorts(IBlueprint blueprint, long id) {
            return Array.Empty<Port>();
        }
    }

}
