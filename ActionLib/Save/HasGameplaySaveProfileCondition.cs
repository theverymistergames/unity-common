using System;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Save;
using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.ActionLib.Save {
    
    [Serializable]
    public sealed class HasGameplaySaveProfileCondition : IActorCondition {

        [Min(0)] public int index;

        public bool IsMatch(IActor context) {
            return Services.TryGet(out IGameplaySaveService service) &&
                   service.HasSavedProfile(service.GetProfileKey(index));
        }
    }
    
}