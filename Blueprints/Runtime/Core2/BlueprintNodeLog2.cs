using System;
using MisterGames.Blueprints.Ports;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public sealed class BlueprintNodeLog2 : BlueprintNode, IBlueprintEnter, IBlueprintOutput<string> {

        public override void CreatePorts() {
            AddPort(Port.Enter("Enter"));
            AddPort(Port.Exit("Exit"));
            AddPort(Port.Input<string>("Input string"));
            AddPort(Port.Output<string>("Output string"));
        }

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
