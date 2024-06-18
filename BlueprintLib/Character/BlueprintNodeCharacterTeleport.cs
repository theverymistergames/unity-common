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
            meta.AddPort(id, Port.Input<Vector3>("Rotation"));
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;
            
            var characterAccess = CharacterAccessRegistry.Instance.GetCharacterAccess(spawnIfNotRegistered: true);
            var collisions = characterAccess.GetComponent<CharacterCollisionPipeline>();
            
            collisions.enabled = false;
            
            var body = characterAccess.GetComponent<CharacterBodyAdapter>();
            var head = characterAccess.GetComponent<CharacterBodyAdapter>();
            
            body.Position = blueprint.Read<Vector3>(token, 1);
            body.Rotation = Quaternion.Euler(blueprint.Read<Vector3>(token, 2));
            
            head.LocalRotation = Quaternion.identity;
            
            collisions.enabled = true;
        }
    }

}
