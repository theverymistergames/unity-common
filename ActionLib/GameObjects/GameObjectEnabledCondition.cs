using System;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.ActionLib.GameObjects {
    
    [Serializable]
    public sealed class GameObjectEnabledCondition : IActorCondition {

        public GameObject gameObject;
        public bool shouldBeActiveSelf;
        public Optional<bool> shouldBeActiveInHierarchy;
        
        public bool IsMatch(IActor context) {
            return shouldBeActiveSelf == gameObject.activeSelf && 
                   shouldBeActiveInHierarchy.IsEmptyOrEquals(gameObject.activeInHierarchy);
        }
    }
    
}