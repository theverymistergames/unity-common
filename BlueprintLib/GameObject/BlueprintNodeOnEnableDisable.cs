using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "On Enable Disable", Category = "GameObject", Color = BlueprintColors.Node.Events)]
    public sealed class BlueprintNodeOnEnableDisable :
        BlueprintNode,
        IBlueprintEnableDisable,
        IBlueprintStart,
        IBlueprintOutput<bool>
    {
        [SerializeField] private bool _invokeFirstEnableOnStart = true;

        public override Port[] CreatePorts() => new[] {
            Port.Exit("On Enable"),
            Port.Exit("On Disable"),
            Port.Output<bool>("Is Enabled"),
        };

        private bool _isEnabled;
        private bool _isFirstEnable = true;

        public void OnEnable() {
            if (_invokeFirstEnableOnStart && _isFirstEnable) return;

            _isFirstEnable = false;
            _isEnabled = true;
            Ports[0].Call();
        }

        public void OnDisable() {
            _isEnabled = false;
            Ports[1].Call();
        }

        public void OnStart() {
            if (!_invokeFirstEnableOnStart) return;

            _isFirstEnable = false;
            _isEnabled = true;
            Ports[0].Call();
        }

        public bool GetOutputPortValue(int port) => port switch {
            2 => _isEnabled,
            _ => false,
        };
    }

}
