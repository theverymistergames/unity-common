using System;
using MisterGames.Blueprints;
using MisterGames.Character.Motion;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceSetForceMultiplier :
        BlueprintSource<BlueprintNodeSetForceZoneMultiplier>,
        BlueprintSources.IEnter<BlueprintNodeSetForceZoneMultiplier>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Set Force Zone Multiplier", Category = "Character", Color = BlueprintColors.Node.Actions)]
    public struct BlueprintNodeSetForceZoneMultiplier : IBlueprintNode, IBlueprintEnter {

        [SerializeField] private float _multiplier;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter());
            meta.AddPort(id, Port.Input<CharacterForceZone>("Zone"));
            meta.AddPort(id, Port.Input<float>("Multiplier"));
        }
        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;

            var zone = blueprint.Read<CharacterForceZone>(token, 1);
            float multiplier = blueprint.Read(token, 2, _multiplier);

            zone.ForceMultiplier = multiplier;
        }
    }

}
