using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core2;

namespace Core {

    [Serializable]
    public sealed class BlueprintNodeTestFactory : BlueprintFactory<BlueprintNodeTestData> {
        public override IBlueprintNode CreateNode() => new BlueprintNodeTest();
    }

    [Serializable]
    public struct BlueprintNodeTestData {
        public int intValue;
        public float floatValue;
    }

    [Serializable]
    public sealed class BlueprintNodeTest : IBlueprintNode {

        public void OnCreateNode(IBlueprint blueprint, long id) { }

        public Port[] CreatePorts(IBlueprint blueprint, long id) {
            return Array.Empty<Port>();
        }
    }

}
