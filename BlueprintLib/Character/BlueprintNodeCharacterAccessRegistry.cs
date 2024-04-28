using System;
using MisterGames.Actors;
using MisterGames.Blueprints;
using MisterGames.Character.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNode(Name = "Character Access Registry", Category = "Character", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeCharacterAccessRegistry : 
        IBlueprintNode, 
        IBlueprintOutput<IActor>, 
        IBlueprintOutput<GameObject>, 
        IBlueprintOutput<Transform> 
    {
        [SerializeField] private bool _spawnHeroIfNotSpawnedYet;
        
        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Output<IActor>());
            meta.AddPort(id, Port.Output<GameObject>());
            meta.AddPort(id, Port.Output<Transform>());
        }

        IActor IBlueprintOutput<IActor>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            return port == 0 ? CharacterAccessRegistry.Instance.GetCharacterAccess(_spawnHeroIfNotSpawnedYet) : default;
        }
        
        GameObject IBlueprintOutput<GameObject>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            return port == 1 ? CharacterAccessRegistry.Instance.GetCharacterAccess(_spawnHeroIfNotSpawnedYet)?.GameObject : default;
        }
        
        Transform IBlueprintOutput<Transform>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            return port == 1 ? CharacterAccessRegistry.Instance.GetCharacterAccess(_spawnHeroIfNotSpawnedYet)?.Transform : default;
        }
    }

}
