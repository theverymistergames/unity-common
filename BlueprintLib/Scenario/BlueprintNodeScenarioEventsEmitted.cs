using System;
using MisterGames.Blueprints;
using MisterGames.Common.Data;
using MisterGames.Scenario.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Scenario Events Emitted", Category = "Scenario", Color = BlueprintLibColors.Node.Scenario)]
    public class BlueprintNodeScenarioEventsEmitted : BlueprintNode, IBlueprintOutput<bool> {

        [SerializeField] private Pair<ScenarioEvent, bool>[] _events;

        public override Port[] CreatePorts() => new[] {
            Port.Output<bool>()
        };

        public bool GetPortValue(int port) {
            if (port != 0) return false;

            for (int i = 0; i < _events.Length; i++) {
                var entry = _events[i];
                var evt = entry.First;
                bool expectedEmitState = entry.Second;

                if (evt.IsEmitted != expectedEmitState) return false;
            }

            return true;
        }
    }

}
