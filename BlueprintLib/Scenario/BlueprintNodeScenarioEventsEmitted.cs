using System;
using MisterGames.Blueprints;
using MisterGames.Common.Data;
using MisterGames.Scenario.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceScenarioEventsEmitted :
        BlueprintSource<BlueprintNodeScenarioEventsEmitted2>,
        BlueprintSources.IOutput<BlueprintNodeScenarioEventsEmitted2, bool>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Scenario Events Emitted", Category = "Scenario", Color = BlueprintLibColors.Node.Scenario)]
    public struct BlueprintNodeScenarioEventsEmitted2 : IBlueprintNode, IBlueprintOutput2<bool> {

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

    [Serializable]
    [BlueprintNodeMeta(Name = "Scenario Events Emitted", Category = "Scenario", Color = BlueprintLibColors.Node.Scenario)]
    public class BlueprintNodeScenarioEventsEmitted : BlueprintNode, IBlueprintOutput<bool> {

        [SerializeField] private Pair<ScenarioEvent, bool>[] _events;

        public override Port[] CreatePorts() => new[] {
            Port.Output<bool>()
        };

        public bool GetOutputPortValue(int port) {
            if (port != 0) return false;

            for (int i = 0; i < _events.Length; i++) {
                var entry = _events[i];
                var evt = entry.A;
                bool expectedEmitState = entry.B;

                if (evt.IsEmitted != expectedEmitState) return false;
            }

            return true;
        }
    }

}
