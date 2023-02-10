using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Pipe", Category = "Flow", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodePipe : BlueprintNode, IBlueprintPortLinker {

        public override Port[] CreatePorts() => new[] {
            Port.Enter(),
            Port.Exit()
        };

        public int GetLinkedPort(int port) => port switch {
            0 => 1,
            1 => 0,
            _ => -1,
        };
    }

}
