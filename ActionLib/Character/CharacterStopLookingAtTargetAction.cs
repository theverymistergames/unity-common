using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.View;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterStopLookingAtTargetAction : IActorAction {

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            context.GetComponent<CharacterViewPipeline>().StopLookAt();
            return default;
        }
    }
    
}