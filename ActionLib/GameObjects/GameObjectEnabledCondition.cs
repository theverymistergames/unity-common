using System;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using UnityEngine;

namespace MisterGames.ActionLib.GameObjects {
    
    [Serializable]
    public sealed class GameObjectEnabledCondition : IActorCondition {

        public GameObject gameObject;
        public bool shouldBeActiveSelf;
        
        public bool IsMatch(IActor context) {
            return shouldBeActiveSelf == gameObject.activeSelf;
        }
    }
    
}