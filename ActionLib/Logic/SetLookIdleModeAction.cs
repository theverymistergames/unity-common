using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Core;
using MisterGames.Logic.Transforms;

namespace MisterGames.ActionLib.Logic {
    
    [Serializable]
    public sealed class SetLookIdleModeAction : IActorAction {
        
        public LookAtBehaviour lookAtBehaviour;
        public LookAtBehaviour.IdleMode mode;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            lookAtBehaviour.SetIdleMode(mode);
            return default;
        }
    }
    
}