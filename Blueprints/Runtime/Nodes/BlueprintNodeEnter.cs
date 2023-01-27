using System;
using MisterGames.Blueprints.Validation;
using UnityEngine;

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    [BlueprintNodeMeta(Name = "Enter", Category = "External", Color = BlueprintColors.Node.External)]
    public sealed class BlueprintNodeEnter : BlueprintNode, IBlueprintLinker, IBlueprintAssetValidator {

        [SerializeField] private string _parameter;

        public override Port[] CreatePorts() => new[] {
            Port.Enter(_parameter).SetExternal(true),
            Port.Exit()
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
