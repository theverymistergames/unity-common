using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Scenario.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Emit Scenario Event", Category = "Scenario", Color = BlueprintLibColors.Node.Scenario)]
    public sealed class BlueprintNodeEmitScenarioEvent : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] private ScenarioEvent _event;

        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Enter(),
            Port.Exit(),
        };

        void IBlueprintEnter.Enter(int port) {
            if (port != 0) return;
            
            _event.Emit();
            Call(1);
        }
    }

}