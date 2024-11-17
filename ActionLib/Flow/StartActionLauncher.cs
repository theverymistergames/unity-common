using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;

namespace MisterGames.ActionLib.Flow
{
    
    [Serializable]
    public sealed class StartActionLauncher : IActorAction
    {
        public ActionLauncher actionLauncher;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default)
        {
            return actionLauncher.Launch(cancellationToken);
        }
    }
    
}