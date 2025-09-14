using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Service;
using MisterGames.UI.Windows;

namespace MisterGames.ActionLib.UI {
    
    [Serializable]
    public sealed class SetUiWindowStateAction : IActorAction {

        public UiWindow window;
        public UiWindowState state;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            Services.Get<IUiWindowService>().SetWindowState(window, state);
            return UniTask.CompletedTask;
        }
    }
    
}