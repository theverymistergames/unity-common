using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace Core {

    [Serializable]
    public sealed class BlueprintSourceTest2 : BlueprintSource<BlueprintNodeTest2> { }

    [Serializable]
    public struct BlueprintNodeTest2 : IBlueprintNode {

        public int intValue;
        public BlueprintAsset2 objectValue;
        [SerializeReference] public object referenceValue;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) { }
    }

}
