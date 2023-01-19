using System;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    [BlueprintNodeMeta(Name = "Core2.Input", Category = "Core2.External", Color = BlueprintColors.Node.External)]
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
            ownerAsset.BlueprintMeta.InvalidateNode(nodeId);
        }
    }

}
