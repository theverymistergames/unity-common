using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Pipe", Category = "Flow", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodePipe : BlueprintNode, IBlueprintEnter {
        
        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Enter(),
            Port.Exit()
        };

        void IBlueprintEnter.Enter(int port) {
            if (port == 0) Call(1);
        }

    }

}