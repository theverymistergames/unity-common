using System;
using MisterGames.Blueprints;
using MisterGames.Input.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Input Action Filter", Category = "Input", Color = BlueprintLibColors.Node.Input)]
    public sealed class BlueprintNodeInputActionFilter : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] private InputActionFilter _filter;

        public override Port[] CreatePorts() => new[] {
            Port.Action(PortDirection.Input, "Apply"),
            Port.Action(PortDirection.Input, "Release"),
            Port.Action(PortDirection.Output),
        };

        public void OnEnterPort(int port) {
            switch (port) {
                case 0:
                    _filter.Apply();
                    break;

                case 1:
                    _filter.Release();
                    break;
            }

            Ports[2].Call();
        }
    }

}
