using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Service;
using MisterGames.UI.Navigation;

namespace MisterGames.ActionLib.UI {
    
    [Serializable]
    public sealed class UiNavigateBackAction : IActorAction {
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            Services.Get<IUiNavigationService>()?.NavigateBack();
            return UniTask.CompletedTask;
        }
    }
    
}