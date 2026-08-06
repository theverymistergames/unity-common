using System;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.ActionLib.Flow {
    
    [Serializable]
    public sealed class InvertedCondition : IActorCondition {

        [SerializeReference] [SubclassSelector] public IActorCondition condition;
        
        public bool IsMatch(IActor context) {
            return condition != null && !condition.IsMatch(context);
        }
    }
    
}