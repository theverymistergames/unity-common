using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core2;

namespace Core {

    [Serializable]
    public sealed class BlueprintNodeTestFactory : BlueprintNodeFactory<BlueprintNodeTestData> {
        public override IBlueprintNode CreateNode() => new BlueprintNodeTest();
        public override IBlueprintNodeFactory CreateFactory() => new BlueprintNodeTestFactory();
    }

    [Serializable]
    public struct BlueprintNodeTestData {
        public int intValue;
        public float floatValue;
    }

    [Serializable]
    public sealed class BlueprintNodeTest : IBlueprintNode {

        public void OnCreateNode(IBlueprintStorage storage, int id) { }

        public Port[] CreatePorts(IBlueprintStorage storage, int id) {
            return Array.Empty<Port>();
        }
    }

}
