using System;
using UnityEngine;

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    [BlueprintNodeMeta(Name = "Output", Category = "Flow", Color = BlueprintColors.Node.External)]
    public sealed class BlueprintNodeOutput : BlueprintNode, IBlueprintOutput {

        [SerializeField] private string _parameter;
        
        public override Port[] CreatePorts() => new[] {
            Port.Input(),
            Port.Output(_parameter).SetExternal(true)
        };

        public T GetPortValue<T>(int port) {
            return port == 1 ? ReadPort<T>(0) : default;
        }
    }

}
