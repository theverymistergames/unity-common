using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Core;
using MisterGames.Logic.Transforms;

namespace MisterGames.ActionLib.Logic {
    
    [Serializable]
    public sealed class LookAtCharacterAction : IActorAction {
        
        public LookAtBehaviour lookAtBehaviour;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            lookAtBehaviour.LookAt(CharacterSystem.Instance.GetCharacter()?.Transform);
            return default;
        }
    }
    
}