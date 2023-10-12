using System;
using MisterGames.Blueprints.Core2;

namespace Core {

    [Serializable]
    public sealed class BlueprintFactoryTest1 : BlueprintFactory<BlueprintNodeTest1> { }

    [Serializable]
    public struct BlueprintNodeTest1 : IBlueprintNode {

        public int intValue;
        public float floatValue;

        public void CreatePorts(IBlueprintMeta meta, long id) { }
    }

}
