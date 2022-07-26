using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Finish", Category = "Exposed", Color = BlueprintColors.Node.Exposed)]
    public sealed class BlueprintNodeFinish : BlueprintNode, IBlueprintEnter {

        internal override bool HasExposedPorts => true;
        
        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Enter(),
            Port.Exit("On Finish").BuiltIn().Exposed()
        };

        void IBlueprintEnter.Enter(int port) {
            if (port == 0) Call(port: 1);
        }

    }

}