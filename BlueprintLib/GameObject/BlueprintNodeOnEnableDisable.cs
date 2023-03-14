using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "On Enable Disable", Category = "GameObject", Color = BlueprintColors.Node.Events)]
    public sealed class BlueprintNodeOnEnableDisable : BlueprintNode, IBlueprintEnableDisable, IBlueprintOutput<bool> {
        
        public override Port[] CreatePorts() => new[] {
            Port.Action(PortDirection.Output, "On Enable"),
            Port.Action(PortDirection.Output, "On Disable"),
            Port.Func<bool>(PortDirection.Output, "Is Enabled"),
        };

        private bool _isEnabled;

        public void OnEnable() {
            _isEnabled = true;
            Ports[0].Call();
        }

        public void OnDisable() {
            _isEnabled = false;
            Ports[1].Call();
        }

        public bool GetOutputPortValue(int port) => port switch {
            2 => _isEnabled,
            _ => false,
        };
    }

}
