using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Labels;
using MisterGames.Common.Save;
using MisterGames.Common.Service;

namespace MisterGames.ActionLib.Save {
    
    [Serializable]
    public sealed class GameplaySaveBoolValueAction : IActorAction {

        public LabelValue key;
        public bool value;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (Services.TryGet(out IGameplaySaveService service)) {
                service.Set(key.GetFullLabel(), 0, value);
            }

            return default;
        }
    }
    
}