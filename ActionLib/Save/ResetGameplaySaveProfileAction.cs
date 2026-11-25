using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Save;
using MisterGames.Common.Service;

namespace MisterGames.ActionLib.Save {

    [Serializable]
    public sealed class ResetGameplaySaveProfileAction : IActorAction {

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (Services.TryGet(out IGameplaySaveService service)) {
                service.ResetCurrentProfile();
            }

            return default;
        }
    }

}
