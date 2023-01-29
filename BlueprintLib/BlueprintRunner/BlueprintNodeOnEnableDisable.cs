﻿using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "On Enable Disable", Category = "Blueprint Runner", Color = BlueprintColors.Node.Events)]
    public sealed class BlueprintNodeOnEnableDisable : BlueprintNode, IBlueprintEnableDisable, IBlueprintOutput<bool> {
        
        public override Port[] CreatePorts() => new[] {
            Port.Exit("On Enable"),
            Port.Exit("On Disable"),
            Port.Output<bool>("Is Enabled"),
        };

        private bool _isEnabled;

        public void OnEnable() {
            _isEnabled = true;
            CallPort(0);
        }

        public void OnDisable() {
            _isEnabled = false;
            CallPort(1);
        }

        public bool GetPortValue(int port) => port switch {
            2 => _isEnabled,
            _ => false,
        };
    }

}