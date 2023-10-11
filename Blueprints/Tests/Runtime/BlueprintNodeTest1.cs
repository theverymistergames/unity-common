using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core2;

namespace Core {

    [Serializable]
    public sealed class BlueprintFactoryTest1 : BlueprintFactory<BlueprintNodeTest1> { }

    [Serializable]
    public struct BlueprintNodeTest1 : IBlueprintNode {

        public int intValue;
        public float floatValue;

        public Port[] CreatePorts(IBlueprintMeta blueprintMeta, long id) {
            return Array.Empty<Port>();
        }
    }

}
