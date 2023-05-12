using System;
using MisterGames.Blueprints;
using MisterGames.Character.Motion;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Set Force Zone Multiplier", Category = "Character", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeSetForceZoneMultiplier : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] private float _multiplier;
        
        public override Port[] CreatePorts() => new[] {
            Port.Enter(),
            Port.Input<CharacterForceZone>("Zone"),
            Port.Input<float>("Multiplier"),
        };

        public void OnEnterPort(int port) {
            if (port != 0) return;

            var zone = Ports[1].Get<CharacterForceZone>();
            float multiplier = Ports[2].Get(_multiplier);

            zone.ForceMultiplier = multiplier;
        }
    }

}
