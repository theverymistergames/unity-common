using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Core;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class UseCharacterContextAction : IActorAction {
        
        [SerializeReference] [SubclassSelector] public IActorAction action;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            return action?.Apply(CharacterSystem.Instance.GetCharacter(), cancellationToken) ?? default;
        }
    }
    
}