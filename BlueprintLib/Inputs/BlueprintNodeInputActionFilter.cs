using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Input.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Input Action Filter", Category = "Input", Color = BlueprintLibColors.Node.Input)]
    public sealed class BlueprintNodeInputActionFilter : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] private InputActionFilter _filter;

        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Enter("Apply"),
            Port.Enter("Release"),
            Port.Exit(),
        };

        void IBlueprintEnter.Enter(int port) {
            switch (port) {
                case 0:
                    _filter.Apply();
                    Call(2);
                    break;
                
                case 1:
                    _filter.Release();
                    Call(2);
                    break;
            }
        }
    }

}