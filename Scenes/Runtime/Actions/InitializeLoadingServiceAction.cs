using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Data;
using MisterGames.Common.Easing;
using MisterGames.Common.Service;
using MisterGames.Scenes.Core;
using MisterGames.Scenes.Loading;
using MisterGames.Scenes.Utils;
using UnityEngine;

namespace MisterGames.Scenes.Actions {
    
    [Serializable]
    public sealed class InitializeLoadingServiceAction : ISceneLoaderAction {

        public UniTask Apply(CancellationToken cancellationToken) {
            Services.Get<ILoadingService>().Initialize();
            return UniTask.CompletedTask;
        }
    }
    
}