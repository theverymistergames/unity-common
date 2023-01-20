﻿using System;
using MisterGames.Blueprints;
using MisterGames.Scenario.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

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
            CallPort(1);
        }
    }

}
