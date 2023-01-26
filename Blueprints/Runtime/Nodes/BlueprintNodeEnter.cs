using System;
using MisterGames.Blueprints.Validation;
using UnityEngine;

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    [BlueprintNodeMeta(Name = "Enter", Category = "External", Color = BlueprintColors.Node.External)]
    public sealed class BlueprintNodeEnter : BlueprintNode, IBlueprintLinker, IBlueprintValidatedNode {

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

        public void OnValidate(int nodeId, BlueprintAsset ownerAsset) {
            ownerAsset.BlueprintMeta.InvalidateNodePorts(nodeId, invalidateLinks: false);
        }
    }

}
