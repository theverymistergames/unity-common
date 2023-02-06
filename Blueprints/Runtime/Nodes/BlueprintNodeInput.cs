using System;
using MisterGames.Blueprints.Runtime.Core;
using MisterGames.Blueprints.Validation;
using UnityEngine;

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    [BlueprintNodeMeta(Name = "Input", Category = "External", Color = BlueprintColors.Node.External)]
    public sealed class BlueprintNodeInput :
        BlueprintNode,
        IBlueprintPortLinker,
        IBlueprintAssetValidator
    {
        [SerializeField] private string _parameter;

        public override Port[] CreatePorts() => new[] {
            Port.Input(_parameter).SetExternal(true),
            Port.Output()
        };

        public int GetLinkedPort(int port) => port switch {
            0 => 1,
            1 => 0,
            _ => -1,
        };

        public void ValidateBlueprint(BlueprintAsset ownerAsset, int nodeId) {
            ownerAsset.BlueprintMeta.InvalidateNodePorts(nodeId, invalidateLinks: false);
        }
    }

}
