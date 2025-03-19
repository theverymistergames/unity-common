using System;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.ActionLib.Flow {
    
    [Serializable]
    public sealed class RandomCondition : IActorCondition {
        
        [Range(0f, 1f)] public float probability;
        
        public bool IsMatch(IActor context, float startTime) {
            return probability >= Random.value;
        }
    }
}
