using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Service;
using MisterGames.Scenes.Core;
using MisterGames.Scenes.Loading;

namespace MisterGames.Scenes.Actions {
    
    [Serializable]
    public sealed class InitializeLoadingServiceAction : ISceneLoaderAction {

        public UniTask Apply(CancellationToken cancellationToken) {
            Services.Get<ILoadingService>().Initialize();
            return UniTask.CompletedTask;
        }
    }
    
}