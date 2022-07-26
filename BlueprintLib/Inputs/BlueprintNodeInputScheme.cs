using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Input.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Input Scheme", Category = "Input", Color = BlueprintLibColors.Node.Input)]
    public sealed class BlueprintNodeInputScheme : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] private InputChannel _channel;
        [SerializeField] private InputScheme _scheme;

        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Enter("Activate"),
            Port.Enter("Deactivate"),
            Port.Exit(),
        };

        void IBlueprintEnter.Enter(int port) {
            switch (port) {
                case 0:
                    _channel.ActivateInputScheme(_scheme);
                    Call(2);
                    break;
                
                case 1:
                    _channel.DeactivateInputScheme(_scheme);
                    Call(2);
                    break;
            }
        }
    }

}