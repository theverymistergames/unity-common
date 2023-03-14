using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "If", Category = "Flow", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodeIf : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] private bool _condition;

        public override Port[] CreatePorts() => new[] {
            Port.Action(PortDirection.Input),
            Port.Func<bool>(PortDirection.Input, "Condition"),
            Port.Action(PortDirection.Output, "On True"),
            Port.Action(PortDirection.Output, "On False"),
        };

        public void OnEnterPort(int port) {
            if (port != 0) return;

            bool condition = Ports[1].Get(_condition);
            Ports[condition ? 2 : 3].Call();
        }
    }

}
