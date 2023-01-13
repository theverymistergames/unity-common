using System;
using MisterGames.Blueprints.Core2;

namespace MisterGames.BlueprintLib.Core2 {

    [Serializable]
    [BlueprintNodeMeta(Name = "Core2.Log", Category = "Core2.Test", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeLog : BlueprintNode, IBlueprintEnter, IBlueprintOutput<string> {

        public override Port[] CreatePorts() => new[] {
            Port.Enter("Enter"),
            Port.Exit("Exit"),
            Port.Input<string>("Input string"),
            Port.Output<string>("Output string"),
        };

        public void OnEnterPort(int port) {
            if (port == 0) CallPort(1);
        }

        public string GetPortValue(int port) {
            return port switch {
                3 => "SomeLog",
                _ => default
            };
        }
    }

}
