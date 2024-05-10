using System;
using MisterGames.Blueprints;
using MisterGames.Character.Collisions;
using MisterGames.Character.Core;
using MisterGames.Character.Motion;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNode(Name = "Character Teleport", Category = "Character", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeCharacterTeleport : IBlueprintNode, IBlueprintEnter {
        
        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter());
            meta.AddPort(id, Port.Input<Vector3>("Position"));
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;
            
            var characterAccess = CharacterAccessRegistry.Instance.GetCharacterAccess(spawnIfNotRegistered: true);
            var collisions = characterAccess.GetComponent<CharacterCollisionPipeline>();
            
            collisions.enabled = false;
            characterAccess.GetComponent<CharacterBodyAdapter>().Position = blueprint.Read<Vector3>(token, 1);
            collisions.enabled = true;
        }
    }

}
