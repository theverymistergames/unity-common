using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Save;
using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.ActionLib.Save {
    
    [Serializable]
    public sealed class LoadGameplaySaveProfileAction : IActorAction {

        [Min(0)] public int index;
        public bool makeCurrent = true;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            return Services.TryGet(out IGameplaySaveService service) 
                ? service.LoadOrCreateProfile(service.GetProfileKey(index), makeCurrent) 
                : default;
        }
    }
    
}