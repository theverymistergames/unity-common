using System;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Blueprints;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.BlueprintLib {
    
    [Serializable]
    [BlueprintNode(Name = "Actor Condition", Category = "Character", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeCharacterCondition : IBlueprintNode, IBlueprintOutput<bool> {

        [SerializeReference] [SubclassSelector] private ActorCondition _condition;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Input<IActor>());
            meta.AddPort(id, Port.Input<IActorCondition>());
            meta.AddPort(id, Port.Output<bool>("Condition"));
        }

        public bool GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 2) return default;

            var actor = blueprint.Read<IActor>(token, 0);
            var condition = blueprint.Read<IActorCondition>(token, 1, _condition);
            
            return condition?.IsMatch(actor) ?? false;
        }
    }

}
