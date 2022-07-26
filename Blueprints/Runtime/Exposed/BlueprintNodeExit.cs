using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Exit", Category = "Exposed", Color = BlueprintColors.Node.Exposed)]
    public sealed class BlueprintNodeExit : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] private string _parameter;
        
        internal override bool HasExposedPorts => true;
        
        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Enter(),
            Port.Exit(_parameter).Exposed(),
        };

        void IBlueprintEnter.Enter(int port) {
            if (port == 0) Call(port: 1);
        }

    }

}