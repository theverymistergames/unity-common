using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Common.Data;
using MisterGames.Scenario.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Scenario Events Emitted", Category = "Scenario", Color = BlueprintLibColors.Node.Scenario)]
    public class BlueprintNodeScenarioEventsEmitted : BlueprintNode, IBlueprintGetter<bool> {

        [SerializeField] private Pair<ScenarioEvent, bool>[] _events;

        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Output<bool>(),
        };

        bool IBlueprintGetter<bool>.Get(int port) {
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