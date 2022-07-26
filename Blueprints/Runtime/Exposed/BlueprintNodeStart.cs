using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Start", Category = "Exposed", Color = BlueprintColors.Node.Exposed)]
    public sealed class BlueprintNodeStart : BlueprintNode, IBlueprintEnter {

        internal override bool HasExposedPorts => true;
        
        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Enter("Start").BuiltIn().Exposed(),
            Port.Exit(),
        };

        void IBlueprintEnter.Enter(int port) {
            if (port == 0) Call(port: 1);
        }

        internal override void OnStart() {
            Call(port: 1);
        }
        
    }

}