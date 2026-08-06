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
    public sealed class DeleteGameplaySaveProfileAction : IActorAction {

        [Min(0)] public int index;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (Services.TryGet(out IGameplaySaveService service)) {
                service.DeleteProfile(service.GetProfileKey(index));
            }

            return default;
        }
    }
    
}