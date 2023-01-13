using System;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public sealed class BlueprintNodeLog2 : BlueprintNode, IBlueprintEnter, IBlueprintOutput<string> {

        public override Port[] CreatePorts() => new[] {
            Ports.Enter("Enter"),
            Ports.Exit("Exit"),
            Ports.Input<string>("Input string"),
            Ports.Output<string>("Output string"),
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
