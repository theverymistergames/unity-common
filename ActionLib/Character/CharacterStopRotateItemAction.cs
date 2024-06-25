using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.View;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterStopRotateItemAction : IActorAction {

        public Transform item;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            context.GetComponent<CharacterViewPipeline>().StopRotateObject(item);
            return default;
        }
    }
    
}