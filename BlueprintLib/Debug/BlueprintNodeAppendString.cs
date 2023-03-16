using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Append String", Category = "Debug", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeAppendString : BlueprintNode, IBlueprintOutput<string> {

        [SerializeField] private string _a;
        [SerializeField] private string _b;

        public override Port[] CreatePorts() => new[] {
            Port.Input<string>("A"),
            Port.Input<string>("B"),
            Port.Output<string>()
        };

        public string GetOutputPortValue(int port) => port switch {
            2 => $"{Ports[0].Get(_a)}{Ports[1].Get(_b)}",
            _ => ""
        };
    }

}
