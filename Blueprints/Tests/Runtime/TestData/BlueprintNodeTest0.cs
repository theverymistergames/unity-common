using System;
using MisterGames.Blueprints;

namespace Core {

    [Serializable]
    public sealed class BlueprintSourceTest0 : BlueprintSource<BlueprintNodeTest0> { }

    [Serializable]
    public struct BlueprintNodeTest0 : IBlueprintNode {

        public int intValue;
        public float floatValue;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter());
            meta.AddPort(id, Port.Exit());
        }
    }

}
