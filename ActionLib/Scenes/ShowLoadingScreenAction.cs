using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Service;
using MisterGames.Scenes.Loading;

namespace MisterGames.ActionLib.Scenes {
    
    [Serializable]
    public sealed class ShowLoadingScreenAction : IActorAction {

        public bool show;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            Services.Get<ILoadingService>().ShowLoadingScreen(show);
            return default;
        }
    }
    
}