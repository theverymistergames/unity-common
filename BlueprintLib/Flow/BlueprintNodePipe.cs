using System;
using MisterGames.Blueprints;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Pipe", Category = "Flow", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodePipe : BlueprintNode, IBlueprintEnter {

        public override Port[] CreatePorts() => new[] {
            Port.Enter(),
            Port.Exit()
        };

        public void OnEnterPort(int port) {
            if (port == 0) CallPort(1);
        }
    }

}
