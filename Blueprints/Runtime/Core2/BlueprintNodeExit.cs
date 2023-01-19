using System;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    [BlueprintNodeMeta(Name = "Core2.Exit", Category = "Core2.External", Color = BlueprintColors.Node.External)]
    public sealed class BlueprintNodeExit : BlueprintNode, IBlueprintEnter, IBlueprintValidatedNode {
        
        [SerializeField] private string _parameter;

        public override Port[] CreatePorts() => new[] {
            Port.Enter(),
            Port.Exit(_parameter).SetExternal(true)
        };

        public void OnEnterPort(int port) {
            if (port == 0) CallPort(1);
        }

        public void OnValidate(int nodeId, BlueprintAsset ownerAsset) {
            ownerAsset.BlueprintMeta.InvalidateNode(nodeId);
        }
    }

}
