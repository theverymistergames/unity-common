using System;
using MisterGames.Blueprints;
using MisterGames.Common.Data;
using MisterGames.Scenario.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceScenarioEventsEmitted :
        BlueprintSource<BlueprintNodeScenarioEventsEmitted>,
        BlueprintSources.IOutput<BlueprintNodeScenarioEventsEmitted, bool>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Scenario Events Emitted", Category = "Scenario", Color = BlueprintLibColors.Node.Scenario)]
    public struct BlueprintNodeScenarioEventsEmitted : IBlueprintNode, IBlueprintOutput<bool> {

        [SerializeField] private Pair<ScenarioEvent, bool>[] _events;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Output<bool>());
        }

        public bool GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return false;

            for (int i = 0; i < _events.Length; i++) {
                (var evt, bool expectedEmitState) = _events[i];
                if (evt.IsEmitted != expectedEmitState) return false;
            }

            return true;
        }
    }

}
