using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Logic.Transforms;

namespace MisterGames.ActionLib.Logic {
    
    [Serializable]
    public sealed class StopLookAtAction : IActorAction {
        
        public LookAtBehaviour lookAtBehaviour;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            lookAtBehaviour.StopLookAt();
            return default;
        }
    }
    
}