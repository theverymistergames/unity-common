using System;

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    internal class BlueprintSourceRoot : BlueprintSource<BlueprintNodeRoot>, BlueprintSources.ICloneable { }

    [Serializable]
    internal struct BlueprintNodeRoot : IBlueprintNode {
        public void CreatePorts(IBlueprintMeta meta, NodeId id) { }
    }

}
