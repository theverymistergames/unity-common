using System;
using MisterGames.Blueprints;
using MisterGames.Input.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Input Scheme", Category = "Input", Color = BlueprintLibColors.Node.Input)]
    public sealed class BlueprintNodeInputScheme : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] private InputChannel _channel;
        [SerializeField] private InputScheme _scheme;

        public override Port[] CreatePorts() => new[] {
            Port.Enter("Activate"),
            Port.Enter("Deactivate"),
            Port.Exit(),
        };

        public void OnEnterPort(int port) {
            switch (port) {
                case 0:
                    _channel.ActivateInputScheme(_scheme);
                    break;

                case 1:
                    _channel.DeactivateInputScheme(_scheme);
                    break;
            }

            Ports[2].Call();
        }
    }

}
