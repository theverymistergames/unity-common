using System;
using MisterGames.Blueprints.Core2;

namespace Core {

    [Serializable]
    public sealed class BlueprintFactoryTest0 : BlueprintFactory<BlueprintNodeTest0> { }

    [Serializable]
    public struct BlueprintNodeTest0 : IBlueprintNode {

        public int intValue;
        public float floatValue;

        public void CreatePorts(IBlueprintMeta meta, long id) { }
    }

}
