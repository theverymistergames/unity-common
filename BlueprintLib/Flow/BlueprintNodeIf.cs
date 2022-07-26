using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "If", Category = "Flow", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodeIf : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] private bool _defaultCondition;
        
        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Enter(),
            Port.Input<bool>("Condition"),
            Port.Exit("On True"),
            Port.Exit("On False"),
        };

        void IBlueprintEnter.Enter(int port) {
            if (port != 0) return;

            bool condition = Read(port: 1, _defaultCondition);
            Call(port: condition ? 2 : 3);
        }

    }

}