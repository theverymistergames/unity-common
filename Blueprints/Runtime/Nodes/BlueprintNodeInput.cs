using System;
using MisterGames.Blueprints.Validation;
using UnityEngine;

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    [BlueprintNodeMeta(Name = "Input", Category = "External", Color = BlueprintColors.Node.External)]
    public sealed class BlueprintNodeInput : BlueprintNode, IBlueprintOutput, IBlueprintValidatedNode {

        [SerializeField] private string _parameter;

        public override Port[] CreatePorts() => new[] {
            Port.Input(_parameter).SetExternal(true),
            Port.Output()
        };

        public T GetPortValue<T>(int port) {
            return port == 1 ? ReadPort<T>(0) : default;
        }

        public void OnValidate(int nodeId, BlueprintAsset ownerAsset) {
            ownerAsset.BlueprintMeta.InvalidateNodePortsAndLinks(nodeId, this);
        }
    }

}
