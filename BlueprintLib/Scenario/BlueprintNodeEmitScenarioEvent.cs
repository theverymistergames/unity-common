using System;
using MisterGames.Blueprints;
using MisterGames.Scenario.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceEmitScenarioEvent :
        BlueprintSource<BlueprintNodeEmitScenarioEvent2>,
        BlueprintSources.IEnter<BlueprintNodeEmitScenarioEvent2>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Emit Scenario Event", Category = "Scenario", Color = BlueprintLibColors.Node.Scenario)]
    public struct BlueprintNodeEmitScenarioEvent2 : IBlueprintNode, IBlueprintEnter2 {

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

    [Serializable]
    [BlueprintNodeMeta(Name = "Emit Scenario Event", Category = "Scenario", Color = BlueprintLibColors.Node.Scenario)]
    public sealed class BlueprintNodeEmitScenarioEvent : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] private ScenarioEvent _event;

        public override Port[] CreatePorts() => new[] {
            Port.Enter(),
            Port.Exit(),
        };

        public void OnEnterPort(int port) {
            if (port != 0) return;

            _event.Emit();
            Ports[1].Call();
        }
    }

}
