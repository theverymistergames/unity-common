using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Blueprints.Nodes;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceOnEnableDisable :
        BlueprintSource<BlueprintNodeOnEnableDisable2>,
        BlueprintSources.IStartCallback<BlueprintNodeOnEnableDisable2>,
        BlueprintSources.IEnableCallback<BlueprintNodeOnEnableDisable2>,
        BlueprintSources.IOutput<BlueprintNodeOnEnableDisable2, bool>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "On Enable Disable", Category = "GameObject", Color = BlueprintColors.Node.Events)]
    public struct BlueprintNodeOnEnableDisable2 :
        IBlueprintNode,
        IBlueprintEnableCallback,
        IBlueprintStartCallback,
        IBlueprintOutput2<bool>
    {
        [SerializeField] private bool _invokeFirstEnableOnStart;

        private bool _isEnabled;
        private bool _wasEnabledOnce;

        public void OnSetDefaults(IBlueprintMeta meta, NodeId id) {
            _invokeFirstEnableOnStart = true;
        }

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Exit("On Enable"));
            meta.AddPort(id, Port.Exit("On Disable"));
            meta.AddPort(id, Port.Output<bool>("Is Enabled"));
        }

        public void OnEnable(IBlueprint blueprint, NodeToken token, bool enabled) {
            if (enabled) {
                if (_invokeFirstEnableOnStart && !_wasEnabledOnce) return;

                _wasEnabledOnce = true;
                _isEnabled = true;

                blueprint.Call(token, 0);
                return;
            }

            _isEnabled = false;
            blueprint.Call(token, 1);
        }

        public void OnStart(IBlueprint blueprint, NodeToken token) {
            if (!_invokeFirstEnableOnStart) return;

            _wasEnabledOnce = true;
            _isEnabled = true;

            blueprint.Call(token, 0);
        }

        public bool GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            2 => _isEnabled,
            _ => false,
        };
    }

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
