using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core2;

namespace Core {

    [Serializable]
    public sealed class BlueprintSourceTest2 : BlueprintSource<BlueprintNodeTest2> { }

    [Serializable]
    public struct BlueprintNodeTest2 : IBlueprintNode {

        public void CreatePorts(IBlueprintMeta meta, long id) { }
    }

}
