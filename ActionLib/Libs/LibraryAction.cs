using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Labels;

namespace MisterGames.ActionLib.Libs {
    
    [Serializable]
    public sealed class LibraryAction : IActorAction {
        
        public LabelValue<IActorAction> action;
        public bool waitAction = true;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (waitAction) {
                return action.GetData()?.Apply(context, cancellationToken) ?? UniTask.CompletedTask;
            }
            
            action.GetData()?.Apply(context, cancellationToken).Forget();
            return default;
        }
    }
    
}