using System;
using MisterGames.Blueprints.Validation;
using UnityEngine;

namespace MisterGames.Blueprints.External {

    [Serializable]
    [BlueprintNodeMeta(Name = "Output", Category = "External", Color = BlueprintColors.Node.External)]
    public sealed class BlueprintNodeOutput : BlueprintNode, IBlueprintOutput, IBlueprintValidatedNode {

        [SerializeField] private string _parameter;
        
        public override Port[] CreatePorts() => new[] {
            Port.Input(),
            Port.Output(_parameter).SetExternal(true)
        };

        public T GetPortValue<T>(int port) {
            return port == 1 ? ReadPort<T>(0) : default;
        }

        public void OnValidate(int nodeId, BlueprintAsset ownerAsset) {
            ownerAsset.BlueprintMeta.InvalidateNodePortsAndLinks(nodeId, this);
        }
    }

}
