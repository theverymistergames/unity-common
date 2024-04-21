using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MisterGames.Actors.Actions {

    [Serializable]
    public sealed class ActionReference : IActorAction {

        public ActorAction actorAction;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            return actorAction == null ? default : actorAction.Apply(context, cancellationToken);
        }
    }

}
