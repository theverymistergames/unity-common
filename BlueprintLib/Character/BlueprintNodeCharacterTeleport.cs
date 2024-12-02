using System;
using MisterGames.Blueprints;
using MisterGames.Character.Core;
using MisterGames.Character.Motion;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNode(Name = "Character Teleport", Category = "Character", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeCharacterTeleport : IBlueprintNode, IBlueprintEnter {
        
        [SerializeField] private bool _preserveVelocity = true;
        
        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter());
            meta.AddPort(id, Port.Input<Vector3>("Position"));
            meta.AddPort(id, Port.Input<Vector3>("Rotation"));
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;

            var motion = CharacterSystem.Instance.GetCharacter().GetComponent<CharacterMotionPipeline>();
            
            motion.Teleport(
                blueprint.Read<Vector3>(token, 1), 
                Quaternion.Euler(blueprint.Read<Vector3>(token, 2)),
                _preserveVelocity
            );
        }
    }

}
