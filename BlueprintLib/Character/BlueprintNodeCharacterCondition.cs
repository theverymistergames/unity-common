using System;
using MisterGames.Blueprints;
using MisterGames.Character.Conditions;
using MisterGames.Character.Core;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNode(Name = "Character Condition", Category = "Character", Color = BlueprintColors.Node.Actions)]
    public class BlueprintNodeCharacterCondition : IBlueprintNode, IBlueprintOutput<bool> {

        [SerializeReference] [SubclassSelector] private ICharacterCondition _condition;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Input<ICharacterAccess>());
            meta.AddPort(id, Port.Output<bool>("Condition"));
        }

        public bool GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 1) return default;

            var characterAccess = blueprint.Read<ICharacterAccess>(token, 0);
            return _condition?.IsMatched(characterAccess) ?? false;
        }
    }

}
