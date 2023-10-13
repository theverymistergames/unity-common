using System;
using MisterGames.Blueprints.Core2;

namespace Core {

    [Serializable]
    public sealed class BlueprintSourceTest1 : BlueprintSource<BlueprintNodeTest1> { }

    [Serializable]
    public struct BlueprintNodeTest1 : IBlueprintNode {

        public int intValue;
        public float floatValue;

        public void CreatePorts(IBlueprintMeta meta, long id) { }
    }

}
