using System;
using MisterGames.Blueprints;
using MisterGames.Scenario.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceEmitScenarioEvent :
        BlueprintSource<BlueprintNodeEmitScenarioEvent>,
        BlueprintSources.IEnter<BlueprintNodeEmitScenarioEvent>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Emit Scenario Event", Category = "Scenario", Color = BlueprintLibColors.Node.Scenario)]
    public struct BlueprintNodeEmitScenarioEvent : IBlueprintNode, IBlueprintEnter {

        [SerializeField] private ScenarioEvent _event;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter());
            meta.AddPort(id, Port.Exit());
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;

            _event.Emit();
            blueprint.Call(token, 1);
        }
    }

}
