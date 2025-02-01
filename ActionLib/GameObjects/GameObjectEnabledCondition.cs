using System;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using UnityEngine;

namespace MisterGames.ActionLib.GameObjects {
    
    [Serializable]
    public sealed class GameObjectEnabledCondition : IActorCondition {

        public GameObject gameObject;
        public bool shouldBeActiveSelf;
        
        public bool IsMatch(IActor context, float startTime) {
            return shouldBeActiveSelf == gameObject.activeSelf;
        }
    }
    
}